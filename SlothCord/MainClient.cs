using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SlothCord.Commands;
using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
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
        public event OnCommandError CommandErrored;
        public event OnMessageCreate MessageCreated;
        public event OnPresenceUpdate PresenceUpdated;
        public event OnGuildsDownloaded GuildsDownloaded;
        public event OnGuildAvailable GuildAvailable;
        public event OnUnknownEvent UnknownEvent;
        public event OnWebSocketClose SocketClosed;
        public event OnHeartbeat Heartbeated;
        public event OnReady ClientReady;
        public event OnClientError ClientErrored;
        public event OnSocketDataReceived SocketErrored;
        public CommandService Commands { get; internal set; } = new CommandService();

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
        /// Prefix for commands
        /// </summary>
        public string StringPrefix { get; set; }

        /// <summary>
        /// The type of token passed
        /// </summary>
        public TokenType TokenType { get; set; }

        /// <summary>
        /// List of guilds the Client is in
        /// </summary>
        public IReadOnlyList<DiscordGuild> Guilds { get; internal set; }

        /// <summary>
        /// Whether or not to compress the payload
        /// </summary>
        public bool Compress { get; set; } = false;

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

        public int MessageCacheLimit { get; set; } = 300;

        public int UserCacheLimit { get; set; } = 300;

        /// <summary>
        /// How many users have to be in a guild before it's considered large
        /// </summary>
        public int LargeThreashold { get; set; } = 250;

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

        public async Task<DiscordUser> GetUserAsync(ulong user_id)
        {
            var response = await _httpClient.GetAsync(new Uri($"{_baseAddress}/users/{user_id}"));
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordUser>(content);
            else return null;
        }

        private void WebSocketClient_Closed(object sender, EventArgs e)
        {
            if (LogActions)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Websocket CLOSED");
                Console.ForegroundColor = ConsoleColor.White;
            }
            SocketClosed?.Invoke(this, e);
        }

        private void WebSocketClient_Error(object sender, ErrorEventArgs e)
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
                            if (Compress) Guilds = await this.GetUserGuildsAsync() as IReadOnlyList<DiscordGuild>;
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
                                            member.Roles = member.RoleIds.Select(x => guild.Roles.First(a => a.Id == x)) as IReadOnlyList<DiscordRole>;
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
                                        if (LogActions)
                                        {
                                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Received PRESENCE UPDATE");
                                            Console.ForegroundColor = ConsoleColor.White;
                                        }
                                        var pl = JsonConvert.DeserializeObject<PresencePayload>(data.EventPayload.ToString());
                                        var guild = this.Guilds.First(x => x.Id == pl.GuildId);
                                        var member = guild.Members.First(x => x.UserData.Id == pl.User.Id);
                                        var prevmember = member;
                                        var args = new PresenceUpdateArgs() { MemberBefore = prevmember };
                                        member.UserData = pl.User;
                                        member.Nickname = pl.Nickname;
                                        member.UserData.Status = pl.Status;
                                        member.Roles = pl.RoleIds.Select(x => guild.Roles.First(a => a.Id == x)) as IReadOnlyList<DiscordRole>;
                                        member.UserData.Game = pl.Game;
                                        args.MemberAfter = member;
                                        PresenceUpdated?.Invoke(this, args);
                                        if(this.EnableUserCaching)
                                            if (this.InternalUserCache != null)
                                            {
                                                this.InternalUserCache[this.InternalUserCache.IndexOf(prevmember.UserData)] = pl.User;
                                                this.CachedUsers = this.InternalUserCache;
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
                                        if (msg.Content.StartsWith(this.StringPrefix))
                                        {
                                            List<object> Args = new List<object>();
                                            Args.AddRange(msg.Content.Replace(this.StringPrefix, "").Split(' ').ToList());
                                            var cmd = Commands.UserDefinedCommands.FirstOrDefault(x => x.CommandName == (Args[0] as string));
                                            if (cmd == null)
                                            {
                                                CommandErrored?.Invoke(this, "Command does not exist");
                                                return;
                                            }
                                            var guild = this.Guilds.FirstOrDefault(x => x.Channels.Any(a => a.Id == msg.ChannelId));
                                            var channel = guild.Channels.First(x => x.Id == msg.ChannelId);
                                            Args.Remove(Args[0]);
                                            var passargs = new List<object>();
                                            int checkammount = 0;
                                            var countval = cmd.Parameters.Count();
                                            if (cmd.Parameters.FirstOrDefault()?.ParameterType == typeof(SlothCommandContext))
                                            {
                                                passargs.Add(new SlothCommandContext()
                                                {
                                                    Channel = channel,
                                                    Guild = guild,
                                                    User = msg.Author,
                                                    Client = this
                                                });
                                                countval--;
                                            }
                                            for (var i = 0; i < countval; i++)
                                            {
                                                checkammount = (passargs.Count - 1);
                                                object currentarg = (Args.Count > checkammount) ? Args[i] : null;
                                                if (currentarg != null)
                                                {
                                                    if (new Regex(@"(<@(?:!)\d+>)").IsMatch(currentarg as string))
                                                    {
                                                        var strid = new Regex(@"((<@)(?:!))").Replace(Args[i] as string, "").Replace(">", "");
                                                        var id = ulong.Parse(strid);
                                                        var member = guild.Members.FirstOrDefault(x => x.UserData.Id == id);
                                                        var cachedUser = InternalUserCache?.FirstOrDefault(x => x.Id == id);
                                                        if (member != null && currentarg.GetType() == typeof(DiscordGuildMember)) currentarg = member;
                                                        else if (cachedUser != null) currentarg = cachedUser;
                                                    }
                                                    else
                                                    {
                                                        var type = cmd.Parameters[i].ParameterType;
                                                        if (type != typeof(SlothCommandContext))
                                                            currentarg = Convert.ChangeType(Args[i], type);
                                                    }
                                                }
                                                var check = cmd.Parameters[i].CustomAttributes.Any(y => y.AttributeType == typeof(RemainingStringAttribute));
                                                if ((bool)check)
                                                {
                                                    var sb = new StringBuilder();
                                                    for (var o = 0; o < Args.Count; o++)
                                                        if (Args.IndexOf(Args[o]) >= Args.IndexOf(Args[i]))
                                                            sb.Append($" {Args[o]}");
                                                    passargs.Add(sb.ToString());
                                                    break;
                                                }
                                                passargs.Add(currentarg);
                                            }
                                            try
                                            {
                                                cmd.Method.Invoke(cmd.ClassInstance, passargs.ToArray());
                                            }
                                            catch (TargetParameterCountException)
                                            {
                                                CommandErrored?.Invoke(cmd.ClassInstance, "Required parameter does not have a value");
                                            }
                                            catch
                                            {
                                                throw;
                                            }
                                        }
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
                                        var guild = Guilds?.FirstOrDefault(x => x.Channels.Any(z => z.Id == chid)) ?? null;
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
                                        var channel = (Guilds.FirstOrDefault(x => x.Channels.Any(a => a.Id == pl.ChnanelId))).Channels.First(x => x.Id == pl.ChnanelId);
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

        internal async Task<IEnumerable<DiscordGuild>> GetUserGuildsAsync()
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

        internal Task SendIdentifyAsync()
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

        internal async void SendHeartbeats()
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
    }

    public class ApiBase
    {
        internal static HttpClient _httpClient = new HttpClient();
        internal static WebSocket WebSocketClient { get; set; }
        internal static Uri _baseAddress = new Uri("https://discordapp.com/api/v6");
    }

    public class GuildMethods : ApiBase
    {
        public event OnHttpError HttpError;

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
    }

    public class ChannelMethods : ApiBase
    {
        public event OnHttpError HttpError;

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

        internal async Task<DiscordChannel> DeleteGuildChannelAsync(ulong channel_id)
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
}