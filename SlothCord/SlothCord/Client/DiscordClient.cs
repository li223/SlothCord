using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SlothCord.Commands;
using SlothCord.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;

namespace SlothCord
{
    public partial class DiscordClient : ApiBase
    {
        public async Task ConnectAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Token)) throw new NullReferenceException("You must supply a valid token");
            if (TokenType != TokenType.Bot) throw new Exception("Only Bot tokens are currently supported");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bot {this.Token}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", $"DiscordBot ($https://github.com/li223/SlothCord, ${this.Version})");
            var response = await _httpClient.GetAsync($"{_baseAddress}/gateway/bot").ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var data = JObject.Parse(content);
            if (!response.IsSuccessStatusCode) throw new Exception($"Gateway returned: {data["message"].ToString()}");
            var shards = int.Parse(data["shards"].ToString());
            var url = data["url"].ToString();
            WebSocketClient = new WebSocket(url);
            WebSocketClient.MessageReceived += WebSocketClient_MessageReceived;
            WebSocketClient.Opened += WebSocketClient_Opened;
            WebSocketClient.Closed += WebSocketClient_Closed;
#if NETCORE
            await WebSocketClient.OpenAsync().ConfigureAwait(false);
#else
            WebSocketClient.Open();
#endif
        }

#pragma warning disable CS1998
        private async void WebSocketClient_Closed(object sender, object e)
        {
            if(FailedReconnects == 5) throw new Exception("Failed to re-establish the connection, abandoning attempts");
            var data = e as ClosedEventArgs;
            FailedReconnects++;
            _heartbeat = false;
            if(!CancellationToken.IsCancellationRequested) CancellationToken.Cancel();
            this.SocketClosed?.Invoke($"Received Close Code: {data?.Code.ToString() ?? "No code"}").ConfigureAwait(false);
            if (data?.Code == 1000)
            {
                _sessionId = "";
                _sequence = null;
            }
            else
            {
#if NETCORE
                switch (WebSocketClient.State)
                {
                    case WebSocketState.Connecting:
                        await Task.Delay(500).ConfigureAwait(false);
                        await SendResumeAsync().ConfigureAwait(false);
                        await WebSocketClient.OpenAsync().ConfigureAwait(false);
                        break;
                    case WebSocketState.Closed:
                        await WebSocketClient.CloseAsync().ConfigureAwait(false);
                        await Task.Delay(500).ConfigureAwait(false);
                        await WebSocketClient.OpenAsync().ConfigureAwait(false);
                        break;
                    case WebSocketState.Open:
                        await WebSocketClient.CloseAsync().ConfigureAwait(false);
                        await Task.Delay(500).ConfigureAwait(false);
                        await WebSocketClient.OpenAsync().ConfigureAwait(false);
                        break;
                }
#else
                switch (WebSocketClient.State)
                {
                    case WebSocketState.Closed:
                        WebSocketClient.Open();
                        break;
                    case WebSocketState.Connecting:
                        WebSocketClient.Close();
                        await Task.Delay(500).ConfigureAwait(false);
                        WebSocketClient.Open();
                        break;
                    case WebSocketState.Open:
                        WebSocketClient.Close();
                        await Task.Delay(500).ConfigureAwait(false);
                        WebSocketClient.Open();
                        break;
                }
#endif
            }
        }
#pragma warning restore CS1998

        private void WebSocketClient_Opened(object sender, EventArgs e)
        {
            if(CancellationToken.IsCancellationRequested) CancellationToken = new CancellationTokenSource();
            this.SocketOpened?.Invoke().ConfigureAwait(false);
        }

        private Task SendResumeAsync()
        {
            var Content = new ResumePayload()
            {
                SessionId = this._sessionId,
                Token = this.Token
            };

            var pldata = new GatewayEvent()
            {
                Code = OPCode.Resume,
                EventPayload = Content
            };

            var payload = JsonConvert.SerializeObject(Content);
            WebSocketClient.Send(payload);
            return Task.CompletedTask;
        }

        private Task SendIdentifyAsync()
        {
            var Content = new IdentifyPayload()
            {
                Token = $"{this.TokenType} {this.Token}",
                Properties = new Properties(),
                Compress = false,
                LargeThreashold = this.LargeThreashold,
                Shard = new[] { 0, 1 }
            };

            var pldata = new GatewayEvent()
            {
                Code = OPCode.Identify,
                EventPayload = Content
            };

            var payload = JsonConvert.SerializeObject(pldata);
            WebSocketClient.Send(payload);
            return Task.CompletedTask;
        }

        public async Task HeartbeatLoop(int inter)
        {
            while (_heartbeat)
            {
                await Task.Delay(inter).ConfigureAwait(false);
                if (WebSocketClient.State == WebSocketState.Open)
                {
                    PingStopwatch.Reset();
                    PingStopwatch.Start();
                    WebSocketClient.Send(@"{""op"":1, ""t"":null,""d"":null,""s"":null}");
                }
                else break;
            }
        }

#pragma warning disable CS4014
        private async void WebSocketClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var data = JsonConvert.DeserializeObject<GatewayEvent>(e.Message);
            _sequence = data.Sequence;
            switch (data.Code)
            {
                case OPCode.Hello:
                    {
                        var hello = JsonConvert.DeserializeObject<GatewayHello>(data.EventPayload.ToString());
                        if (string.IsNullOrWhiteSpace(_sessionId))
                        {
                            await SendIdentifyAsync().ConfigureAwait(false);
                            Task.Run(async() => await HeartbeatLoop(hello.HeartbeatInterval)).ConfigureAwait(false);
                            this._heartbeatInterval = hello.HeartbeatInterval;
                        }
                        else await SendResumeAsync().ConfigureAwait(false);
                        break;
                    }
                case OPCode.InvalidateSession:
                    {
                        _heartbeat = false;
                        if ((bool)data.EventPayload)
                        {
                            await Task.Delay(500).ConfigureAwait(false);
                            await SendResumeAsync().ConfigureAwait(false);
                            await HeartbeatLoop(_heartbeatInterval).ConfigureAwait(false);
                        }
                        else
                        {
                            _sessionId = "";
                            await SendIdentifyAsync().ConfigureAwait(false);
                            await HeartbeatLoop(_heartbeatInterval).ConfigureAwait(false);
                        }
                        break;
                    }
                case OPCode.HeartbeatAck:
                    {
                        Ping = this.PingStopwatch.ElapsedMilliseconds;
                        this.PingStopwatch.Stop();
                        this.Heartbeated?.Invoke().ConfigureAwait(false);
                        break;
                    }
                case OPCode.Dispatch:
                    {
                        try
                        {
#if NETCORE
                            var okay = Enum.TryParse(typeof(DispatchType), data.EventName?.Replace("_", ""), true, out object res);
                            if (res == null || !okay)
                            {
                                this.UnknownEventReceived?.Invoke($"Unknown Dispatch Type: {(int)data.Code}", $"\n{data.EventPayload}").ConfigureAwait(false);
                                break;
                            }
                            await HandleDispatchEventAsync((DispatchType)res, data.EventPayload.ToString()).ConfigureAwait(false);
#else
                        var okay = Enum.TryParse(data.EventName.Replace("_", "").ToLower(), out DispatchType res);
                        if (!okay)
                        {
                            this.UnknownEventReceived?.Invoke($"Unknown Dispatch Type: {(int)data.Code}", $"\n{data.EventPayload}").ConfigureAwait(false);
                            break;
                        }
                        await HandleDispatchEventAsync(res, data.EventPayload.ToString()).ConfigureAwait(false);
#endif
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(ex.Message + @"\r\n\r\n" + ex.StackTrace);
                        }
                        break;
                    }
                default:
                    {
                        this.UnknownEventReceived?.Invoke($"Unknown OpCode: {data.EventName} ({(int)data.Code})", data.EventPayload.ToString()).ConfigureAwait(false);
                        break;
                    }
            }
        }
#pragma warning restore CS4014

        private async Task HandleDispatchEventAsync(DispatchType code, string payload)
        {
            switch (code)
            {
                case DispatchType.Ready:
                    {
                        var ready = JsonConvert.DeserializeObject<ReadyPayload>(payload);
                        _sessionId = ready.SessionId;
                        _guildsToDownload = ready.Guilds.Count();
                        this.Ready?.Invoke().ConfigureAwait(false);
                        break;
                    }
                case DispatchType.GuildMemberUpdate:
                    {
                        var member = JsonConvert.DeserializeObject<DiscordGuildMember>(payload);
                        member = this.Guilds.First(x => x.Id == member.GuildId).Members.FirstOrDefault(x => x.UserData.Id == member.UserData.Id);
                        this.MemberUpdated?.Invoke(member).ConfigureAwait(false);
                        break;
                    }
                case DispatchType.TypingStart:
                    {
                        var typing = JsonConvert.DeserializeObject<TypingStartPayload>(payload);
                        var guild = this.Guilds.FirstOrDefault(x => x.Id == (typing.GuildId ?? 0));
                        object channel;
                        if (guild != null) channel = guild.Channels.FirstOrDefault(x => x.Id == typing.ChannelId);
                        else channel = this.PrivateChannels.FirstOrDefault(x => x.Id == typing.ChannelId);
                        this.TypingStarted?.Invoke(typing.UserId, channel).ConfigureAwait(false);
                        break;
                    }
                case DispatchType.PresenceUpdate:
                    {
                        var presence = JsonConvert.DeserializeObject<PresencePayload>(payload);
                        var guild = this.Guilds.FirstOrDefault(x => x.Id == presence.GuildId);
                        if(guild != null)
                        {
                            var member = guild.Members.FirstOrDefault(x => x.UserData.Id == presence.User.Id);
                            member.RoleIds = presence.RoleIds;
                            member.Nickname = presence.Nickname;
                            member.UserData.Status = presence.Status;
                            member.UserData.Activity = presence.Activity;
                            this.PresenceUpdated?.Invoke(guild, member).ConfigureAwait(false);
                        }
                        break;
                    }
                case DispatchType.GuildCreate:
                    {
                        var guild = JsonConvert.DeserializeObject<DiscordGuild>(payload);
                        this._internalGuilds.Add(guild);
                        this._downloadedGuilds++;
                        foreach(var member in guild.Members)
                        {
                            member.Guild = guild;
                            member.Roles = guild.Roles.Where(x => member.RoleIds.Any(a => a == x?.Id)) as IReadOnlyList<DiscordGuildRole?>;
                        }
                        this.GuildCreated?.Invoke(guild).ConfigureAwait(false);
                        if (this._guildsToDownload == this._downloadedGuilds)
                        {
                            this.Guilds = this._internalGuilds;
                            this.PrivateChannels = await GetPrivateChannelsAsync().ConfigureAwait(false);
                            this.GuildsDownloaded?.Invoke(this.Guilds).ConfigureAwait(false);
                        }  
                        break;
                    }
                case DispatchType.MessageCreate:
                    {
                        var msg = JsonConvert.DeserializeObject<DiscordMessage>(payload);
                        this.MessageReceived?.Invoke(msg).ConfigureAwait(false);
                        if (msg.Content.StartsWith(CommandsProvider.Prefix))
                        {
                            var args = await CommandsProvider.ParseArguementsAsync(msg).ConfigureAwait(false);
                            Command? cmd;
                            bool sub = false;
                            cmd = CommandsProvider.CommandsList.FirstOrDefault(x => (x?.CommandName == args[0]) || (x?.Aliases?.FirstOrDefault(a => a == args[0]) != null));
                            if (cmd == null)
                            {
                                var group = CommandsProvider.GroupCommandsList.FirstOrDefault(x => (x?.GroupName == args[0]) || (x?.Aliases?.FirstOrDefault(a => a == args[0]) != null));
                                cmd = group?.SubCommands.FirstOrDefault(x => (x.CommandName == args[1]) || (x.Aliases?.FirstOrDefault(a => a == args[1]) != null));
                                if (cmd == null && group?.ExecuteMethod != null)
                                    cmd = new Command()
                                    {
                                        Method = group?.ExecuteMethod,
                                        MethodParams = group?.ExecuteMethod.GetParameters()
                                    };
                                else if (cmd != null) sub = true;
                                else break;
                            }
                            await CommandsProvider.ExecuteCommandAsync(args, (Command)cmd, sub, this, msg);
                        }
                        break;
                    }
                case DispatchType.GuildMemberAdd:
                    {
                        var usr = JsonConvert.DeserializeObject<DiscordGuildMember>(payload);
                        this.MemberAdded?.Invoke(this.Guilds.FirstOrDefault(x => x.Id == usr.GuildId), usr).ConfigureAwait(false);
                        break;
                    }
                case DispatchType.GuildMemberRemove:
                    {
                        var data = JsonConvert.DeserializeObject<MemberRemovedPayload>(payload);
                        this.MemberRemoved?.Invoke(this.Guilds.FirstOrDefault(x => x.Id == data.GuildId), data.User).ConfigureAwait(false);
                        break;
                    }
                case DispatchType.Resumed:
                    {
                        this.GatewayResumed?.Invoke().ConfigureAwait(false);
                        break;
                    }
                    default:
                    {
                        this.UnknownEventReceived?.Invoke($"Unknown Dispatch Type: {(int)code}", $"\n{payload}").ConfigureAwait(false);
                        break;
                    }
            }
        }
    }

    public partial class DiscordClient : ApiBase
    {
        public event ReadyEvent Ready;
        public event HeartbeatedEvent Heartbeated;
        public event SocketOpenedEvent SocketOpened;
        public event SocketClosedEvent SocketClosed;
        public event GuildsDownloadedEvent GuildsDownloaded;
        public event GuildCreatedEvent GuildCreated;
        public event UnkownEvent UnknownEventReceived;
        public event MessageCreatedEvent MessageReceived;
        public event MemberAddedEvent MemberAdded;
        public event MemberRemovedEvent MemberRemoved;
        public event ResumedEvent GatewayResumed;
        public event PresenceUpdateEvent PresenceUpdated;
        public event TypingStartEvent TypingStarted;
        public event MemberUpdatedEvent MemberUpdated;

        private List<DiscordGuild> _internalGuilds = new List<DiscordGuild>();
        private bool _heartbeat = true;
        private int _heartbeatInterval = 0;
        private string _sessionId = "";
        private int? _sequence = null;
        private int _guildsToDownload = 0;
        private int _downloadedGuilds = 0;
        private Stopwatch PingStopwatch = new Stopwatch();
        private int FailedReconnects = -1;

        /// <summary>
        /// Heartbeat response time in ms
        /// </summary>
        public long Ping { get; private set; }

        /// <summary>
        /// Your bot token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The type of token passed
        /// </summary>
        public TokenType TokenType { get; set; }

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
        /// Gets the assembly version
        /// </summary>
        public string Version { get => FileVersionInfo.GetVersionInfo(Assembly.GetCallingAssembly().Location).ProductVersion; }

        /// <summary>
        /// Command service used for bot commands
        /// </summary>
        public CommandsProvider CommandsProvider { get; set; }

        /// <summary>
        /// The current client as a user
        /// </summary>
        public DiscordUser CurrentUser { get; internal set; }

        /// <summary>
        /// The current bot application
        /// </summary>
        public DiscordApplication CurrentApplication { get; internal set; }

        public IEnumerable<DiscordGuild> Guilds { get; internal set; }

        public IEnumerable<DiscordChannel> PrivateChannels { get; internal set; }

        public async Task<DiscordUser> GetUserAsync(ulong user_id)
        {
            var msg = new HttpRequestMessage(HttpMethod.Get, new Uri($"{_baseAddress}/users/{user_id}"));
            var response = await _httpClient.SendRequestAsync(msg, RequestType.Other, CancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var user = JsonConvert.DeserializeObject<DiscordUser>(content);
                return user;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                    return JsonConvert.DeserializeObject<DiscordUser>(await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg).ConfigureAwait(false));
                else return null;
            }
        }

        public async Task<DiscordGuild> GetGuildAsync(ulong guild_id)
        {
            var msg = new HttpRequestMessage(HttpMethod.Get, new Uri($"{_baseAddress}/@me/guilds/{guild_id}"));
            var response = await _httpClient.SendRequestAsync(msg, RequestType.Other, CancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<DiscordGuild>(content);
            else
            {
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                    return JsonConvert.DeserializeObject<DiscordGuild>(await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg).ConfigureAwait(false));
                else return null;
            }
        }

        public async Task<IEnumerable<DiscordChannel>> GetPrivateChannelsAsync()
        {
            var msg = new HttpRequestMessage(HttpMethod.Get, new Uri($"{_baseAddress}/users/@me/channels"));
            var response = await _httpClient.SendRequestAsync(msg, RequestType.Other, CancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<IEnumerable<DiscordChannel>>(content);
            else
            {
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                    return JsonConvert.DeserializeObject<IEnumerable<DiscordChannel>>(await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg).ConfigureAwait(false));
                else return null;
            }
        }
    }
}