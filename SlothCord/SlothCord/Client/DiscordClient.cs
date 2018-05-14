using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SlothCord.Commands;
using SlothCord.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net;

namespace SlothCord
{
    public class DiscordClient : ApiBase
    {
        #region Random Stuff
        public event OnHttpError HttpError;
        public event OnMessageUpdate MessageUpdate;
        public event OnChannelEvent PinUpdate;
        public event OnChannelEvent ChannelUpdate;
        public event OnChannelEvent ChannelDelete;
        public event OnChannelEvent ChannelCreate;
        public event OnTypingStart TypingStarted;
        public event OnMessageCreate MessageCreated;
        public event OnPresenceUpdate PresenceUpdated;
        public event OnGenericEvent RoleUpdated;
        public event OnGenericEvent RoleCreated;
        public event OnGenericEvent RoleDeleted;
        public event OnGuildsDownloaded GuildsDownloaded;
        public event OnGuildAvailable GuildAvailable;
        public event OnUnknownEvent UnknownEvent;
        public event OnWebSocketClose SocketClosed;
        public event OnHeartbeat Heartbeated;
        public event OnReady ClientReady;
        public event OnClientError ClientErrored;
        public event OnSocketDataReceived SocketErrored;

        private int _sequence = 0;
        private int _guildsToDownload = 0;
        private int _downloadedGuilds = 0;
        private string _sessionId = "";
        private bool _heartbeat = false;
        private int _heartbeatInterval = 0;

        internal List<DiscordGuild> AvailableGuilds = new List<DiscordGuild>();
        internal List<DiscordUser> InternalUserCache = new List<DiscordUser>();
        internal List<DiscordMessage> InternalMessageCache = new List<DiscordMessage>();

        public IReadOnlyList<DiscordUser> CachedUsers { get; internal set; }
        public IReadOnlyList<DiscordMessage> CachedMessages { get; internal set; }

        /// <summary>
        /// Your bot token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The type of token passed
        /// </summary>
        public TokenType TokenType { get; set; }

        /// <summary>
        /// List of guilds the Client is in
        /// </summary>
        public IReadOnlyList<DiscordGuild> Guilds { get; internal set; }

        /// <summary>
        /// I'm too lazy to let you have a choice in this
        /// </summary>
        internal bool Compress = false;

        /// <summary>
        /// Let the library write to the console
        /// </summary>
        public bool LogActions { get; set; } = false;

        /// <summary>
        /// Whether or not to add users to a collection
        /// </summary>
        public bool EnableUserCaching { get; set; } = true;

        /// <summary>
        /// Whether or not to add users to a collection
        /// </summary>
        public bool EnableMessageCaching { get; set; } = false;

        /// <summary>
        /// Max number of messages that can be held at any given time
        /// </summary>
        public int MessageCacheLimit { get; set; } = 100;

        /// <summary>
        /// Max number of users that can be held at any given time
        /// </summary>
        public int UserCacheLimit { get; set; } = 50;

        /// <summary>
        /// How many users have to be in a guild before it's considered large
        /// </summary>
        public int LargeThreashold { get; set; } = 250;

        /// <summary>
        /// Command service used for bot commands
        /// </summary>
        public CommandService Commands { get; set; }

        /// <summary>
        /// The current client as a user
        /// </summary>
        public DiscordUser CurrentUser { get; internal set; }

        public string VersionString { get => FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(this.GetType()).Location).FileVersion; }
        #endregion

        #region Methods
        /// <summary>
        /// Connect to the websocket
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            if (TokenType != TokenType.Bot)
                throw new NotSupportedException("Only bot tokens are supported");

            if (LogActions)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Sending request to GET /gatway/bot");
                Console.ForegroundColor = ConsoleColor.White;
            }
            if (string.IsNullOrEmpty(this.Token))
                throw new ArgumentException("Token cannot be null");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"{TokenType} {this.Token}");
            this.Token = this.Token;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "DiscordBot ($https://github.com/li223/SlothCord, $2.2.5)");
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"{_baseAddress}/gateway/bot"));
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                if (LogActions)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Action Success");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                var jobj = JsonConvert.DeserializeObject<HttpPayload>(content);
                WebSocketClient = new WebSocket(jobj.WSUrl);
                WebSocketClient.MessageReceived += WebSocketClient_MessageReceived;
                WebSocketClient.Closed += WebSocketClient_Closed;
#if NETCORE
                await WebSocketClient.OpenAsync().ConfigureAwait(false);
#elif NETFX47
                WebSocketClient.Open();
#endif
            }
            else
            {
                if (LogActions)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Action errored: {JObject.Parse(content).SelectToken("message")}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                ClientErrored?.Invoke(this, new ClientErroredArgs()
                {
                    Message = $"Server responded with: {JObject.Parse(content).SelectToken("message")}",
                    Source = "HttpClient"
                });
            }
        }

        private Task SendIdentifyAsync()
        {
            if (LogActions)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Gateway SEND IDENTIFY");
                Console.ForegroundColor = ConsoleColor.White;
            }
            var Content = JObject.Parse(JsonConvert.SerializeObject(new IdentifyPayload()
            {
                Token = $"{this.TokenType} {this.Token}",
                Properties = new Properties(),
                Compress = this.Compress,
                LargeThreashold = this.LargeThreashold,
                Shard = new[] { 0, 1 }
            }));
            var pldata = new GatewayPayload()
            {
                Code = (int)OPCode.Identify,
                EventPayload = Content
            };
            var payload = JsonConvert.SerializeObject(pldata);
            WebSocketClient.Send(payload);
            return Task.CompletedTask;
        }

        private Task SendResumeAsync()
        {
            _heartbeat = false;
            if (LogActions)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Gateway SEND RESUME");
                Console.ForegroundColor = ConsoleColor.White;
            }
            var Content = JsonConvert.SerializeObject(new ResumePayload()
            {
                Token = $"{this.TokenType} {this.Token}",
                SessionId = _sessionId
            });
            var pldata = new GatewayPayload()
            {
                Code = (int)OPCode.Resume,
                EventPayload = Content,
                Sequence = _sequence
            };
            var payload = JsonConvert.SerializeObject(pldata);
            WebSocketClient.Send(payload);
            return Task.CompletedTask;
        }

        private async void SendHeartbeats()
        {
            while (_heartbeat)
            {
                if (WebSocketClient.State == WebSocketState.Open)
                {
                    if (LogActions)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Gateway SEND HEARTBEAT");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    WebSocketClient.Send(@"{""op"":1, ""t"":null,""d"":null,""s"":null}");
                    await Task.Delay(_heartbeatInterval).ConfigureAwait(false);
                }
                else break;
            }
        }

        private async void WebSocketClient_Closed(object sender, EventArgs e)
        {
            if (LogActions)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Websocket CLOSED");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Attempting Reconnect");
                Console.ForegroundColor = ConsoleColor.White;
            }
            var data = JsonConvert.DeserializeObject<OnWebSocketClosedArgs>(JsonConvert.SerializeObject(e));
            _heartbeat = false;
            if (data.Code == CloseCode.GracefulClose)
            {
                _sessionId = "";
                _sequence = 0;
                SocketClosed?.Invoke(this, data);
#if NETCORE
                if (WebSocketClient.State == WebSocketState.Open || WebSocketClient.State == WebSocketState.Connecting)
                    await WebSocketClient.CloseAsync().ConfigureAwait(false);
                await WebSocketClient.OpenAsync().ConfigureAwait(false);
#elif NETFX47
                if (WebSocketClient.State == WebSocketState.Open || WebSocketClient.State == WebSocketState.Connecting)
                    WebSocketClient.Close();
                WebSocketClient.Open();
#endif
            }
            else
            {
                _heartbeat = false;
/*
#if NETCORE
                await WebSocketClient.OpenAsync().ConfigureAwait(false);
#else
                WebSocketClient.Open();
#endif
*/
            }
        }

        private void WebSocketClient_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            if (LogActions)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Websocket ERROR");
                Console.ForegroundColor = ConsoleColor.White;
            }
            SocketErrored?.Invoke(this, e.Exception.Message);
        }

        private async void WebSocketClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (LogActions)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Socket Message Received");
                Console.ForegroundColor = ConsoleColor.White;
            }
            var data = JsonConvert.DeserializeObject<GatewayPayload>(e.Message);
            if (!string.IsNullOrEmpty(data.Sequence.ToString()))
                _sequence = (int)data.Sequence;
            switch (data.Code)
            {
                case (int)OPCode.Hello:
                    {
                        var pl = JsonConvert.DeserializeObject<GatewayHello>(data.EventPayload.ToString());
                        if (LogActions)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Gateway HELLO");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        if (_sessionId == "")
                        {
                            _heartbeatInterval = pl.HeartbeatInterval;
                            _heartbeat = true;
                            var hbt = new Task(SendHeartbeats, TaskCreationOptions.LongRunning);
                            hbt.Start();
                            await SendIdentifyAsync().ConfigureAwait(false);
                        }
                        else
                        {
                            _heartbeatInterval = pl.HeartbeatInterval;
                            _heartbeat = true;
                            var hbt = new Task(SendHeartbeats, TaskCreationOptions.LongRunning);
                            hbt.Start();
                            await SendResumeAsync().ConfigureAwait(false);
                        }
                        break;
                    }
                case (int)OPCode.Dispatch:
                    {
                        if (LogActions)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Gateway DISPATCH");
                        }
#if NETCORE
                        var obj = Enum.TryParse(typeof(DispatchType), data.EventName, out var res);
#elif NETFX47
                        var obj = Enum.TryParse<DispatchType>(data.EventName, out var res);
#endif
                        if (obj)
                        {
                            switch (Enum.Parse(typeof(DispatchType), data.EventName))
                            {
                                case DispatchType.READY:
                                    {
                                        if (LogActions)
                                        {
                                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Received READY");
                                            Console.ForegroundColor = ConsoleColor.White;
                                        }
                                        data.EventPayload = JsonConvert.DeserializeObject<ReadyPayload>(data.EventPayload.ToString());
                                        _guildsToDownload = (data.EventPayload as ReadyPayload).Guilds.Count;
                                        var pl = data.EventPayload as ReadyPayload;
                                        _sessionId = pl.SessionId;
                                        this.CurrentUser = pl.User;
                                        ClientReady?.Invoke(this, new OnReadyArgs()
                                        {
                                            GatewayVersion = pl.Version,
                                            SessionId = pl.SessionId
                                        });
                                        break;
                                    }
                                case DispatchType.GUILD_CREATE:
                                    {
                                        if (LogActions)
                                        {
                                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Received GUILD CREATE");
                                            Console.ForegroundColor = ConsoleColor.White;
                                        }
                                        var guild = JsonConvert.DeserializeObject<DiscordGuild>(data.EventPayload.ToString());
                                        foreach (var member in guild.Members)
                                        {
                                            var roles = new List<DiscordRole>();
                                            foreach (var id in member.RoleIds)
                                                roles.Add(guild.Roles.FirstOrDefault(x => x.Id == id));
                                            member.Roles = roles;
                                            member.GuildId = guild.Id;
                                            member.Guild = guild;
                                        }
                                        AvailableGuilds.Add(guild);
                                        GuildAvailable?.Invoke(this, guild);
                                        _downloadedGuilds++;
                                        if (_guildsToDownload == _downloadedGuilds)
                                        {
                                            this.Guilds = AvailableGuilds;
                                            GuildsDownloaded?.Invoke(this, this.Guilds);
                                        }
                                        if (EnableUserCaching)
                                        {
                                            if (InternalUserCache.Count > UserCacheLimit)
                                                for (var count = guild.Members.Select(x => x.UserData).Count(); count != 0; count--)
                                                    InternalUserCache.RemoveAt(0);
                                            InternalUserCache.AddRange(guild.Members.Select(x => x.UserData));
                                            this.CachedUsers = InternalUserCache;
                                        }
                                        break;
                                    }
                                case DispatchType.PRESENCE_UPDATE:
                                    {
                                        var pl = JsonConvert.DeserializeObject<PresencePayload>(data.EventPayload.ToString());
                                        var guild = this.Guilds.FirstOrDefault(x => x.Id == pl.GuildId);
                                        var guildmembers = guild.Members.ToList();
                                        var member = guildmembers?.FirstOrDefault(x => x.UserData.Id == pl.User.Id);
                                        var user = this.CachedUsers?.FirstOrDefault(x => x.Id == pl.User.Id);
                                        var prevmember = member;
                                        var args = new PresenceUpdateArgs() { MemberBefore = prevmember };
                                        if (pl.Status == StatusType.Online && member == null) member = await guild.GetMemberAsync(pl.User.Id).ConfigureAwait(false);
                                        if (member != null)
                                        {
                                            member.Guild = guild;
                                            member.Nickname = pl.Nickname;
                                            member.UserData.Status = pl.Status;
                                            var roles = new List<DiscordRole>();
                                            foreach (var id in member.RoleIds)
                                                roles.Add(guild.Roles.FirstOrDefault(x => x.Id == id));
                                            member.Roles = roles;
                                            member.UserData.Activity = pl.Activity;
                                        }
                                        else if (user != null) user.Activity = pl.Activity;
                                        args.MemberAfter = member;
                                        if(member != null && prevmember != null) guildmembers[guildmembers.IndexOf(prevmember)] = member;
                                        guild.Members = guildmembers;
                                        PresenceUpdated?.Invoke(this, args);
                                        if (this.EnableUserCaching)
                                            if (this.InternalUserCache != null)
                                            {
                                                var index = this.InternalUserCache.IndexOf(prevmember?.UserData);
                                                if (index > -1)
                                                {
                                                    this.InternalUserCache[index] = pl.User;
                                                    this.CachedUsers = this.InternalUserCache;
                                                }
                                            }
                                        break;
                                    }
                                case DispatchType.MESSAGE_CREATE:
                                    {
                                        if (LogActions)
                                        {
                                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Received MESSAGE CREATE");
                                            Console.ForegroundColor = ConsoleColor.White;
                                        }
                                        var msg = JsonConvert.DeserializeObject<DiscordMessage>(data.EventPayload.ToString());
                                        if (EnableMessageCaching)
                                        {
                                            if (InternalMessageCache.Count > MessageCacheLimit)
                                                for (var count = InternalMessageCache.Count; count != 0; count--)
                                                    InternalMessageCache.RemoveAt(0);
                                            InternalMessageCache.Add(msg);
                                            this.CachedMessages = InternalMessageCache;
                                        }
                                        MessageCreated?.Invoke(this, msg);
                                        if (msg.Content.StartsWith(this.Commands.StringPrefix))
                                            await Commands.ConvertArgumentsAsync(this, msg).ConfigureAwait(false);
                                        break;
                                    }
                                case DispatchType.TYPING_START:
                                    {
                                        if (LogActions)
                                        {
                                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Received TYPING START");
                                            Console.ForegroundColor = ConsoleColor.White;
                                        }
                                        var jobj = JObject.Parse(data.EventPayload.ToString());
                                        var chid = ulong.Parse(jobj["channel_id"].ToString());
                                        var usid = ulong.Parse(jobj["user_id"].ToString());
                                        var guild = Guilds?.FirstOrDefault(x => x?.Channels?.Any(z => z.Id == chid) ?? false);
                                        if (guild != null)
                                        {
                                            TypingStarted?.Invoke(this, new TypingStartArgs()
                                            {
                                                Channel = guild.Channels.FirstOrDefault(x => x.Id == chid),
                                                Member = guild.Members.FirstOrDefault(x => x.UserData.Id == usid),
                                                Guild = guild ?? null,
                                                ChannelId = chid,
                                                UserId = usid
                                            });
                                        }
                                        break;
                                    }
                                case DispatchType.CHANNEL_CREATE:
                                    {
                                        if (LogActions)
                                        {
                                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Received CHANNEL CREATE");
                                            Console.ForegroundColor = ConsoleColor.White;
                                        }
                                        var pl = JsonConvert.DeserializeObject<DiscordChannel>(data.EventPayload.ToString());
                                        ChannelCreate?.Invoke(this, pl);
                                        var list = Guilds as List<DiscordGuild>;
                                        var guild = list.FirstOrDefault(x => x.Channels.Any(a => a.Id == pl.Id));
                                        var channels = guild.Channels as List<DiscordChannel>;
                                        var channel = guild.Channels.FirstOrDefault(x => x.Id == pl.Id);
                                        channels.Add(pl);
                                        guild.Channels = channels;
                                        Guilds = list;
                                        break;
                                    }
                                case DispatchType.CHANNEL_DELETE:
                                    {
                                        if (LogActions)
                                        {
                                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Received CHANNEL DELETE");
                                            Console.ForegroundColor = ConsoleColor.White;
                                        }
                                        var pl = JsonConvert.DeserializeObject<DiscordChannel>(data.EventPayload.ToString());
                                        ChannelDelete?.Invoke(this, pl);
                                        var list = Guilds as List<DiscordGuild>;
                                        var guild = list.FirstOrDefault(x => x.Channels.Any(a => a.Id == pl.Id));
                                        var channels = guild.Channels as List<DiscordChannel>;
                                        var channel = guild.Channels.FirstOrDefault(x => x.Id == pl.Id);
                                        channels.Remove(pl);
                                        guild.Channels = channels;
                                        Guilds = list;
                                        break;
                                    }
                                case DispatchType.CHANNEL_UPDATE:
                                    {
                                        if (LogActions)
                                        {
                                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Received CHANNEL UPDATE");
                                            Console.ForegroundColor = ConsoleColor.White;
                                        }
                                        var pl = JsonConvert.DeserializeObject<DiscordChannel>(data.EventPayload.ToString());
                                        ChannelUpdate?.Invoke(this, pl);
                                        var list = Guilds as List<DiscordGuild>;
                                        var guild = list.FirstOrDefault(x => x.Channels.Any(a => a.Id == pl.Id));
                                        var channels = guild.Channels as List<DiscordChannel>;
                                        var channel = guild.Channels.FirstOrDefault(x => x.Id == pl.Id);
                                        channels[channels.IndexOf(channel)] = pl;
                                        guild.Channels = channels;
                                        Guilds = list;
                                        break;
                                    }
                                case DispatchType.CHANNEL_PINS_UPDATE:
                                    {
                                        if (LogActions)
                                        {
                                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Received CHANNEL PINS UPDATE");
                                            Console.ForegroundColor = ConsoleColor.White;
                                        }
                                        var pl = JsonConvert.DeserializeObject<ChannelPinPayload>(data.EventPayload.ToString());
                                        var channel = (Guilds.FirstOrDefault(x => x.Channels.Any(a => a.Id == pl.ChnanelId))).Channels.FirstOrDefault(x => x.Id == pl.ChnanelId);
                                        PinUpdate?.Invoke(this, channel);
                                        break;
                                    }
                                case DispatchType.MESSAGE_UPDATE:
                                    {
                                        if (LogActions)
                                        {
                                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Received MESSAGE UPDATE");
                                            Console.ForegroundColor = ConsoleColor.White;
                                        }
                                        var pl = JsonConvert.DeserializeObject<DiscordMessage>(data.EventPayload.ToString());
                                        var prevmsg = this.CachedMessages?.FirstOrDefault(x => x.Id == pl.Id);
                                        MessageUpdate?.Invoke(this, prevmsg, pl);
                                        if (this.EnableMessageCaching)
                                            if (this.InternalMessageCache != null)
                                            {
                                                if(prevmsg != null) this.InternalMessageCache[this.InternalMessageCache.IndexOf(prevmsg)] = pl;
                                                else this.InternalMessageCache.Add(pl);
                                                this.CachedMessages = this.InternalMessageCache;
                                            }
                                        break;
                                    }
                                case DispatchType.GUILD_ROLE_UPDATE:
                                    {
                                        if (LogActions)
                                        {
                                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Received ROLE UPDATE");
                                            Console.ForegroundColor = ConsoleColor.White;
                                        }
                                        var pl = JsonConvert.DeserializeObject<KeyValuePair<ulong, DiscordRole>>(data.EventPayload.ToString());
                                        RoleUpdated?.Invoke(this, pl);
                                        var list = Guilds as List<DiscordGuild>;
                                        var guild = list.FirstOrDefault(x => x.Roles.Any(a => a.Id == pl.Value.Id));
                                        var roles = guild.Roles as List<DiscordRole>;
                                        var role = guild.Roles.FirstOrDefault(x => x.Id == pl.Value.Id);
                                        roles[roles.IndexOf(role)] = pl.Value;
                                        guild.Roles = roles;
                                        Guilds = list;
                                        break;
                                    }
                                case DispatchType.GUILD_ROLE_CREATE:
                                    {
                                        if (LogActions)
                                        {
                                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Received ROLE CREATE");
                                            Console.ForegroundColor = ConsoleColor.White;
                                        }
                                        var pl = JsonConvert.DeserializeObject<KeyValuePair<ulong, DiscordRole>>(data.EventPayload.ToString());
                                        RoleCreated?.Invoke(this, pl);
                                        var list = Guilds as List<DiscordGuild>;
                                        var guild = list.FirstOrDefault(x => x.Roles.Any(a => a.Id == pl.Value.Id));
                                        var roles = guild.Roles as List<DiscordRole>;
                                        roles.Add(pl.Value);
                                        guild.Roles = roles;
                                        Guilds = list;
                                        break;
                                    }
                                case DispatchType.GUILD_ROLE_DELETE:
                                    {
                                        if (LogActions)
                                        {
                                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Received ROLE DELETE");
                                            Console.ForegroundColor = ConsoleColor.White;
                                        }
                                        var pl = JsonConvert.DeserializeObject<KeyValuePair<ulong, ulong>>(data.EventPayload.ToString());
                                        RoleDeleted?.Invoke(this, pl);
                                        var list = Guilds as List<DiscordGuild>;
                                        var guild = list.FirstOrDefault(x => x.Roles.Any(a => a.Id == pl.Value));
                                        var roles = guild.Roles as List<DiscordRole>;
                                        var role = roles.FirstOrDefault(x => x.Id == pl.Value);
                                        roles.Remove(role);
                                        guild.Roles = roles;
                                        Guilds = list;
                                        break;
                                    }
                                case DispatchType.VOICE_STATE_UPDATE:
                                    {
                                        var pl = JsonConvert.DeserializeObject<VoiceStateUpdatePaylod>(data.EventPayload.ToString());
                                        var guild = Guilds.FirstOrDefault(x => x.Id == pl.GuildId);
                                        if (guild != null)
                                        {
                                            var member = guild.Members.FirstOrDefault(x => x.UserData.Id == pl.UserId);
                                            if (member != null)
                                            {
                                                var roles = new List<DiscordRole>();
                                                foreach (var id in member.RoleIds)
                                                    roles.Add(guild.Roles.FirstOrDefault(x => x.Id == id));
                                                member.Roles = roles;
                                                member.Guild = guild;
                                                member.ChannelId = pl.ChannelId ?? null;
                                                member.IsDeaf = pl.IsDeaf;
                                                member.IsMute = pl.IsMute;
                                                member.IsSelfDeaf = pl.IsSelfDeaf;
                                                member.IsSelfMute = pl.IsSelfMute;
                                                member.IsMutedByCurrentUser = pl.IsMutedByCurrentUser;
                                            }
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        UnknownEvent?.Invoke(this, new UnkownEventArgs()
                                        {
                                            EventName = data.EventName,
                                            OPCode = data.Code
                                        });
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            if (data.EventName == "RESUMED")
                            {
                                _heartbeat = true;
                                var hbt = new Task(SendHeartbeats, TaskCreationOptions.LongRunning);
                                hbt.Start();
                                break;
                            }
                            if (LogActions)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Received UNKNOWN EVENT ({data.EventName})");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            UnknownEvent?.Invoke(this, new UnkownEventArgs()
                            {
                                EventName = data.EventName,
                                OPCode = data.Code
                            });
                        }
                        break;
                    }
                case (int)OPCode.HeartbeatAck:
                    {
                        if (LogActions)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Gateway HEARTBEAT ACKNOWLEDGE");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        Heartbeated?.Invoke(this, "WebSocket Heartbeat Ack");
                        break;
                    }
                case (int)OPCode.Reconnect:
                    {
                        if (LogActions)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Gateway RECONNECT");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
#if NETCORE
                        await WebSocketClient.CloseAsync().ConfigureAwait(false);
                        await WebSocketClient.OpenAsync().ConfigureAwait(false);
#elif NETFX47
                        WebSocketClient.Close();
                        WebSocketClient.Open();
#endif
                        _heartbeat = true;
                        var tsk = new Task(SendHeartbeats);
                        tsk.Start();
                        break;
                    }
                case (int)OPCode.InvalidateSession:
                    {
                        _heartbeat = false;
                        if (LogActions)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Gateway INVALID SESSION");
                        }
                        if ((bool)data.EventPayload)
                        {
                            await Task.Delay(1000).ConfigureAwait(false);
                            var content = JsonConvert.SerializeObject(new ResumePayload()
                            {
                                Sequence = this._sequence,
                                SessionId = this._sessionId,
                                Token = $"{this.TokenType} {this.Token}"
                            });
                            var jsondata = JsonConvert.SerializeObject(new GatewayPayload()
                            {
                                Code = (int)OPCode.Resume,
                                EventPayload = content
                            });
                            WebSocketClient.Send(jsondata);
                        }
                        else
                        {
                            _sessionId = "";
                            _downloadedGuilds = 0;
                            await SendIdentifyAsync().ConfigureAwait(false);
                        }
                        break;
                    }
                default:
                    {
                        if (LogActions)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Gateway UNKOWN EVENT {data.EventName}");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        UnknownEvent?.Invoke(this, new UnkownEventArgs()
                        {
                            EventName = data.EventName,
                            OPCode = data.Code
                        });
                    }
                    break;
            }
        }

        public async Task<DiscordGuild> GetGuildAsync(ulong guild_id)
        {
            var response = await _httpClient.GetAsync(new Uri($"{_baseAddress}/guilds/{guild_id}")).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<DiscordGuild>(content);
            else return null;
        }

        public async Task<DiscordChannel> DeleteChannelAsync(ulong channel_id)
        {
            var response = await _httpClient.DeleteAsync(new Uri($"{_baseAddress}/channels/{channel_id}")).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordChannel>(content);
            else
            {
                HttpError?.Invoke(this, content);
                return null;
            }
        }

        public async Task<IReadOnlyList<DiscordGuild>> GetUserGuildsAsync()
        {
            var response = await _httpClient.GetAsync(new Uri($"{_baseAddress}/users/@me/guilds")).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<IReadOnlyList<DiscordGuild>>(content);
            else
            {
                HttpError?.Invoke(this, content);
                return null;
            }
        }

        public async Task<DiscordChannel> GetChannelAsync(ulong channel_id)
        {
            var response = await _httpClient.GetAsync(new Uri($"{_baseAddress}/channels/{channel_id}")).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordChannel>(content);
            else
            {
                HttpError?.Invoke(this, content);
                return null;
            }
        }

        public async Task<DiscordUser> GetUserAsync(ulong user_id)
        {
            var response = await _httpClient.GetAsync(new Uri($"{_baseAddress}/users/{user_id}")).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordUser>(content);
            else return null;
        }

        public async Task<DiscordGuild> CreateGuildAsync(string name, string region, string icon_file_path, VerificationLevel verificationLevel, NotificationLevel notificationLevel, ExplicitContentFilterLevel explicitContentFilter, IReadOnlyList<DiscordRole> roles, IReadOnlyList<DiscordChannel> channels)
        {
            var guild = new DiscordGuild()
            {
                Name = name,
                Region = region,
                VerificationLevel = verificationLevel,
                DefaultMessageNotifications = notificationLevel,
                ExplicitContentFilter = explicitContentFilter,
                Roles = roles as IReadOnlyList<DiscordRole>,
                Channels = channels as IReadOnlyList<DiscordChannel>
            };
            var response = await _httpClient.PostAsync(new Uri($"{_baseAddress}/guilds"), new StringContent(JsonConvert.SerializeObject(guild))).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordGuild>(content);
            else return null;
        }

        public Task UpdateCurrentUserPresenceAsync(DiscordGame game = null, StatusType status_type = StatusType.Online)
        {
            string status = null;
            switch (status_type)
            {
                case StatusType.Online: status = "online"; break;
                case StatusType.Offline: status = "offline"; break;
                case StatusType.DND: status = "dnd"; break;
                case StatusType.Idle: status = "idle"; break;
            }
            var request = new UserPresencePayload()
            {
                Since = null,
                Game = game,
                Status = status,
                Afk = false
            };
            var pldata = new GatewayPayload()
            {
                Code = 3,
                EventPayload = request
            };
            var payload = JsonConvert.SerializeObject(pldata);
            WebSocketClient.Send(payload);
            return Task.CompletedTask;
        }
#endregion
    }
}