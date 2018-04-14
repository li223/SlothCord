using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SlothCord.Commands;
using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebSocket4Net;

namespace SlothCord
{
    public class DiscordClient : ApiBase
    {
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
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
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
                await WebSocketClient.OpenAsync();
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
                Code = OPCode.Identify,
                EventPayload = Content
            };
            var payload = JsonConvert.SerializeObject(pldata);
            WebSocketClient.Send(payload);
            return Task.CompletedTask;
        }

        private async void SendHeartbeats()
        {
            while (true)
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
                    await Task.Delay(_heartbeatInterval);
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
            SocketClosed?.Invoke(this, e);
            await WebSocketClient.OpenAsync();
        }

        private void WebSocketClient_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            if (LogActions)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  WEbsocket ERROR");
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
                case OPCode.Hello:
                    {
                        if (LogActions)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Gateway HELLO");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        if (_sessionId == "")
                        {
                            var pl = JsonConvert.DeserializeObject<GatewayHello>(data.EventPayload.ToString());
                            _heartbeatInterval = pl.HeartbeatInterval;
                            var hbt = new Task(SendHeartbeats, TaskCreationOptions.LongRunning);
                            hbt.Start();
                            await SendIdentifyAsync();
                        }
                        break;
                    }
                case OPCode.Dispatch:
                    {
                        if (LogActions)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Gateway DISPATCH");
                        }
                        var obj = Enum.TryParse(typeof(DispatchType), data.EventName, out var res);
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
                                            member.Roles = member.RoleIds.Select(x => guild.Roles.FirstOrDefault(a => a.Id == x)) as IReadOnlyList<DiscordRole>;
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
                                        var member = guild.Members?.FirstOrDefault(x => x.UserData.Id == pl.User.Id);
                                        var user = this.CachedUsers?.FirstOrDefault(x => x.Id == pl.User.Id);
                                        var prevmember = member;
                                        var args = new PresenceUpdateArgs() { MemberBefore = prevmember };
                                        if (member != null)
                                        {
                                            member.Guild = guild;
                                            member.UserData = pl.User;
                                            member.Nickname = pl.Nickname;
                                            member.UserData.Status = pl.Status;
                                            var roles = new List<DiscordRole>();
                                            foreach (var id in member.RoleIds)
                                                roles.Add(guild.Roles.FirstOrDefault(x => x.Id == id));
                                            member.Roles = roles;
                                            member.UserData.Game = pl.Game;
                                        }
                                        else
                                        {
                                            user = pl.User;
                                            user.Game = pl.Game;
                                        }
                                        args.MemberAfter = member;
                                        PresenceUpdated?.Invoke(this, args);
                                        if(this.EnableUserCaching)
                                            if (this.InternalUserCache != null)
                                            {
                                                var index = this.InternalUserCache.IndexOf(prevmember.UserData);
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
                                            await Commands.ConvertArgumentsAsync(this, msg);
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
                                        if(this.EnableMessageCaching)
                                            if (this.InternalMessageCache != null)
                                            {
                                                this.InternalMessageCache[this.InternalMessageCache.IndexOf(prevmsg)] = pl;
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
                                                member.Guild = guild;
                                                member.ChannelId = (ulong)pl.ChannelId;
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
                                            OPCode = ((int)data.Code)
                                        });
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            if (LogActions)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Received UNKNOWN EVENT ({data.EventName})");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            UnknownEvent?.Invoke(this, new UnkownEventArgs()
                            {
                                EventName = data.EventName,
                                OPCode = (int)data.Code
                            });
                        }
                        break;
                    }
                case OPCode.HeartbeatAck:
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
                case OPCode.Reconnect:
                    {
                        if (LogActions)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Gateway RECONNECT");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        await WebSocketClient.CloseAsync();
                        await WebSocketClient.OpenAsync();
                        var tsk = new Task(SendHeartbeats);
                        tsk.Start();
                        break;
                    }
                case OPCode.InvalidSession:
                    {
                        if (LogActions)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Gateway INVALID SESSION");
                        }
                        if ((bool)data.EventPayload)
                        {
                            await Task.Delay(1000);
                            var content = JsonConvert.SerializeObject(new ResumePayload()
                            {
                                Sequence = this._sequence,
                                SessionId = this._sessionId,
                                Token = $"{this.TokenType} {this.Token}"
                            });
                            var jsondata = JsonConvert.SerializeObject(new GatewayPayload()
                            {
                                Code = OPCode.Resume,
                                EventPayload = content
                            });
                            WebSocketClient.Send(jsondata);
                        }
                        else
                        {
                            await SendIdentifyAsync();
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
                            OPCode = ((int)data.Code)
                        });
                    }
                    break;
            }
        }

        public async Task<DiscordGuild> GetGuildAsync(ulong guild_id)
        {
            var response = await _httpClient.GetAsync(new Uri($"{_baseAddress}/guilds/{guild_id}"));
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<DiscordGuild>(content);
            else return null;
        }

        public async Task<DiscordChannel> DeleteChannelAsync(ulong channel_id)
        {
            var response = await _httpClient.DeleteAsync(new Uri($"{_baseAddress}/channels/{channel_id}"));
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordChannel>(content);
            else
            {
                HttpError?.Invoke(this, content);
                return null;
            }
        }

        public async Task<IEnumerable<DiscordGuild>> GetUserGuildsAsync()
        {
            var response = await _httpClient.GetAsync(new Uri($"{_baseAddress}/users/@me/guilds"));
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<IEnumerable<DiscordGuild>>(content);
            else
            {
                HttpError?.Invoke(this, content);
                return null;
            }
        }

        public async Task<DiscordChannel> GetChannelAsync(ulong channel_id)
        {
            var response = await _httpClient.GetAsync(new Uri($"{_baseAddress}/channels/{channel_id}"));
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordChannel>(content);
            else
            {
                HttpError?.Invoke(this, content);
                return null;
            }
        }

        public async Task<DiscordUser> GetUserAsync(ulong user_id)
        {
            var response = await _httpClient.GetAsync(new Uri($"{_baseAddress}/users/{user_id}"));
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordUser>(content);
            else return null;
        }

        public async Task<DiscordGuild> CreateGuildAsync(string name, string region, string icon_file_path, VerificationLevel verificationLevel, NotificationLevel notificationLevel, ExplicitContentFilterLevel explicitContentFilter, ICollection<DiscordRole> roles, ICollection<DiscordChannel> channels)
        {
            var guild = new DiscordGuild()
            {
                Name = name,
                Region = region,
                IconUrl = Convert.ToBase64String(await File.ReadAllBytesAsync(icon_file_path)),
                VerificationLevel = verificationLevel,
                DefaultMessageNotifications = notificationLevel,
                ExplicitContentFilter = explicitContentFilter,
                Roles = roles as IReadOnlyList<DiscordRole>,
                Channels = channels as IReadOnlyList<DiscordChannel>
            };
            var response = await _httpClient.PostAsync(new Uri($"{_baseAddress}/guilds"), new StringContent(JsonConvert.SerializeObject(guild)));
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordGuild>(content);
            else return null;
        }
    }

    public class ApiBase
    {
        protected internal static HttpClient _httpClient = new HttpClient();
        protected internal static WebSocket WebSocketClient { get; set; }
        protected internal static Uri _baseAddress = new Uri("https://discordapp.com/api/v6");
    }

    public class MessageMethods : ApiBase
    {
        public event OnHttpError HttpError;

        internal async Task<DiscordMessage> EditDiscordMessageAsync(ulong channel_id, ulong message_id, string content, DiscordEmbed embed)
        {
            if (embed == null && content == null)
            {
                HttpError?.Invoke(this, "Cannot send empty message");
                return null;
            }
            var obj = new MessageUpdatePayload()
            {
                Content = content,
                Embed = embed
            };
            var response = await _httpClient.PutAsync(new Uri($"{_baseAddress}/channels/{channel_id}/messages/{message_id}"), new StringContent(JsonConvert.SerializeObject(obj)));
            var rescont = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordMessage>(rescont);
            else
            {
                HttpError?.Invoke(this, rescont);
                return null;
            }
        }

        internal async Task<DiscordMessage> DeleteMessageAsync(ulong channel_id, ulong message_id)
        {
            var response = await _httpClient.DeleteAsync(new Uri($"{_baseAddress}/channels/{channel_id}/messages/{message_id}"));
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordMessage>(content);
            else
            {
                HttpError?.Invoke(this, content);
                return null;
            }
        }
    }

    public class GuildMethods : ApiBase
    {
        public event OnHttpError HttpError;

        internal async Task LeaveGuildAsync(ulong guild_id)
        {
            var response = await _httpClient.DeleteAsync(new Uri($"{_baseAddress}/users/@me/guilds/{guild_id}"));
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                HttpError?.Invoke(this, content);
        }

        internal async Task CreateBanAsync(ulong guild_id, ulong member_id, int clear_days = 0, string reason = null)
        {
            if (clear_days < 0 || clear_days > 7)
                throw new ArgumentException("Clear days must be between 0 - 7");
            var query = $"{_baseAddress}/guilds/{guild_id}/bans/{member_id}?delete-message-days={clear_days}";
            if (reason != null)
                query += $"&reason={reason}";
            var request = new HttpRequestMessage(HttpMethod.Put, new Uri(query));
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                HttpError?.Invoke(this, content);
        }

        internal async Task<IEnumerable<DiscordGuildMember>> ListGuildMembersAsync(ulong guild_id, int limit = 100, ulong? around = null)
        {
            var requeststring = $"{_baseAddress}/guilds/{guild_id}/members?limit={limit}";
            if (around != null)
                requeststring += $"&around={around}";
            var response = await _httpClient.GetAsync(new Uri(requeststring));
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var members = JsonConvert.DeserializeObject<List<DiscordGuildMember>>(content);
                for(var i = 0; i < members.Count(); i++)  members[i].GuildId = guild_id;
                return members as IEnumerable<DiscordGuildMember>;
            }
            else
            {
                HttpError?.Invoke(this, content);
                return null;
            }
        }

        internal async Task<DiscordGuildMember> ListGuildMemberAsync(ulong guild_id, ulong member_id)
        {
            var response = await _httpClient.GetAsync(new Uri($"{_baseAddress}/guilds/{guild_id}/members/{member_id}"));
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var member = JsonConvert.DeserializeObject<DiscordGuildMember>(content);
                member.GuildId = guild_id;
                return member;
            }
            else
            {
                return null;
            }
        }

        internal async Task<DiscordChannel> GetGuildChannelAsync(ulong guild_id, ulong channel_id)
        {
            var response = await _httpClient.GetAsync(new Uri($"{_baseAddress}/channels/{channel_id}"));
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var channel = JsonConvert.DeserializeObject<DiscordChannel>(content);
                if (channel.GuildId == null || channel.GuildId != guild_id) return null;
                else return channel;
            }
            else
            {
                HttpError?.Invoke(this, content);
                return null;
            }
        }
    }

    public class ChannelMethods : ApiBase
    {
        public event OnHttpError HttpError;

        internal async Task<DiscordInvite> DeleteDiscordInviteAsync(string code, int? with_counts = null)
        {
            var response = await _httpClient.DeleteAsync(new Uri($"{_baseAddress}/invites/{code}"));
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordInvite>(content);
            else
            {
                HttpError?.Invoke(this, content);
                return null;
            }
        }

        internal async Task<DiscordInvite> GetDiscordInviteAsync(string code, int? with_counts = null)
        {
            var query = $"{_baseAddress}/invites/{code}";
            if (with_counts != null)
                query += $"/with_counts/{with_counts}";
            var response = await _httpClient.GetAsync(new Uri(query));
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordInvite>(content);
            else
            {
                HttpError?.Invoke(this, content);
                return null;
            }
        }

        internal async Task BulkDeleteGuildMessagesAsync(ulong? guild_id, ulong channel_id, ICollection<ulong> message_ids)
        {
            if (guild_id == null) return;
            var msgs = new BulkDeletePayload()
            {
                Messages = message_ids.ToArray()
            };
            var response = await _httpClient.PostAsync(new Uri($"{_baseAddress}/channels{channel_id}/messages/bulk-delete"), new StringContent(JsonConvert.SerializeObject(msgs)));
        }

        internal async Task<IEnumerable<DiscordMessage>> GetMultipleMessagesAsync(ulong channel_id, int limit = 50, ulong? around = null, ulong? after = null, ulong? before = null)
        {
            var requeststring = $"{_baseAddress}/channels/{channel_id}/messages?limit={limit}";
            if (around != null)
                requeststring += $"&around={around}";
            if (before != null)
                requeststring += $"&before={before}";
            if (after != null)
                requeststring += $"&after={after}";
            var response = await _httpClient.GetAsync(new Uri(requeststring));
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<IEnumerable<DiscordMessage>>(content);
            else
            {
                HttpError?.Invoke(this, content);
                return null;
            }
        }

        internal async Task<DiscordMessage> GetSingleMessageAsync(ulong channel_id, ulong message_id)
        {
            var response = await _httpClient.GetAsync(new Uri($"{_baseAddress}/channels/{channel_id}/messages/{message_id}"));
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordMessage>(content);
            else
            {
                HttpError?.Invoke(this, content);
                return null;
            }
        }

        internal async Task<DiscordChannel> DeleteChannelAsync(ulong channel_id)
        {
            var response = await _httpClient.DeleteAsync(new Uri($"{_baseAddress}/channels/{channel_id}"));
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordChannel>(content);
            else
            {
                HttpError?.Invoke(this, content);
                return null;
            }
        }

        internal async Task<DiscordMessage> CreateMessageAsync(ulong channel_id, string message = null, bool is_tts = false, DiscordEmbed embed = null)
        {
            if (message?.Length > 2000)
                throw new ArgumentException("Message cannot exceed 2000 characters");
            if (string.IsNullOrEmpty(message) && embed == null)
                throw new Exception("Cannot send an empty message");

            var jsondata = JsonConvert.SerializeObject(new MessageCreatePayload()
            {
                HasContent = message != null,
                Content = message,
                HasEmbed = embed != null,
                Embed = embed,
                IsTTS = is_tts
            });

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"{_baseAddress}/channels/{channel_id}/messages"))
            {
                Content = new StringContent(jsondata, Encoding.UTF8, "application/json")
            };
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordMessage>(content);
            else
            {
                HttpError?.Invoke(this, content);
                return null;
            }
        }
    }

    public class UserMethods : ApiBase
    {
        internal async Task<DiscordChannel> CreateUserDmChannelAsync(ulong user_id)
        {
            var response = await _httpClient.PostAsync($"{_baseAddress}/users/@me/channels?recipient_id={user_id}", null);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordChannel>(content);
            else return null;
        }
    }

    public class MemberMethods : ApiBase
    {
        internal async Task ModifyAsync(ulong guild_id, ulong member_id, string nickname, IEnumerable<DiscordRole> roles, bool? is_muted, bool? is_deaf, ulong? channel_id)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, new Uri($"{_baseAddress}/guilds/{guild_id}/members/{member_id}"))
            {
                Content = new StringContent(JsonConvert.SerializeObject(new MemberModifyPayload()
                {
                    Nickname = nickname,
                    Roles = roles,
                    IsMute = is_muted,
                    IsDeaf = is_deaf,
                    ChannelId = channel_id
                }))
            };
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
        }

        internal async Task<DiscordChannel> CreateUserDmChannelAsync(ulong user_id)
        {
            var response = await _httpClient.PostAsync($"{_baseAddress}/users/@me/channels?recipient_id={user_id}", null);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordChannel>(content);
            else return null;
        }
    }
}