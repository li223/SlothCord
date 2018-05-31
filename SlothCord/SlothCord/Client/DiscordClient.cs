using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SlothCord.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using WebSocket4Net;

namespace SlothCord
{
    public sealed class DiscordClient : ApiBase
    {
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
        public string Version { get => FileVersionInfo.GetVersionInfo(Assembly.GetCallingAssembly().Location).FileVersion; }

        /// <summary>
        /// Command service used for bot commands
        /// </summary>
        public CommandService Commands { get; set; }

        /// <summary>
        /// The current client as a user
        /// </summary>
        public DiscordUser CurrentUser { get; internal set; }

        /// <summary>
        /// The current bot application
        /// </summary>
        public DiscordApplication CurrentApplication { get; internal set; }

        public IReadOnlyList<DiscordGuild> Guilds { get; internal set; }

        private List<DiscordGuild> InternalGuilds { get; set; }

        private bool _heartbeat = true;

        private string _sessionId = "";

        private int? _sequence = null;

        private int GuildsToDownload = 0;

        private int DownloadedGuilds = 0;

        public async Task ConnectAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Token)) throw new ArgumentNullException("You must supply a valid token");
            if (TokenType != TokenType.Bot) throw new ArgumentException("Only Bot tokens are currently supported");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bot {this.Token}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", $"DiscordBot ($https://github.com/li223/SlothCord, ${this.Version})");
            var response = await _httpClient.GetAsync($"{_baseAddress}/gateway/bot").ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var data = JObject.Parse(content);
            if (!response.IsSuccessStatusCode) throw new Exception($"Gateway returned: {data["message"].ToString()}");
            var shards = data["shards"].ToString();
            var url = data["url"].ToString();
            WebSocketClient = new WebSocket(url);
            WebSocketClient.MessageReceived += WebSocketClient_MessageReceived;
#if NETCORE
            await WebSocketClient.OpenAsync().ConfigureAwait(false);
#else
            WebSocketClient.Open();
#endif
        }

        private Task SendIdentifyAsync()
        {
            var Content = JsonConvert.SerializeObject(new IdentifyPayload()
            {
                Token = $"{this.TokenType} {this.Token}",
                Properties = new Properties(),
                Compress = false,
                LargeThreashold = this.LargeThreashold,
                Shard = new[] { 0, 1 }
            });
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
            while(_heartbeat)
            {
                await Task.Delay(inter).ConfigureAwait(false);
                if (WebSocketClient.State == WebSocketState.Open) WebSocketClient.Send(@"{""op"":1, ""t"":null,""d"":null,""s"":null}");
                else break;
            }
        }

        private async void WebSocketClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var data = JsonConvert.DeserializeObject<GatewayEvent>(e.Message);
            _sequence = data.Sequence;
            switch(data.Code)
            {
                case OPCode.Hello:
                    {
                        var hello = JsonConvert.DeserializeObject<GatewayHello>(data.EventPayload);
                        if (_sessionId == "") await HeartbeatLoop(hello.HeartbeatInterval).ConfigureAwait(false);
                        else { /*resume*/ }
                        break;
                    }
                case OPCode.HeartbeatAck:
                    {
                        //Heartbeat Ack Event
                        break;
                    }
                case OPCode.Dispatch:
                    {
                        var type = Enum.TryParse(data.EventName, out DispatchType res);
                        await HandleDispatchEventAsync(res, data.EventPayload).ConfigureAwait(false);
                        break;
                    }
                case OPCode.StatusUpdate:
                    {
                        break;
                    }
                default:
                    {
                        //Unknown opcode event
                        break;
                    }
            }
        }
        
        private async Task HandleDispatchEventAsync(DispatchType code, string payload)
        {
            switch(code)
            {
                case DispatchType.Ready:
                    {
                        var ready = JsonConvert.DeserializeObject<ReadyPayload>(payload);
                        _sessionId = ready.SessionId;
                        GuildsToDownload = ready.Guilds.Count;
                        break;
                    }
                case DispatchType.GuildCreate:
                    {
                        //Guild Create Event
                        var guild = JsonConvert.DeserializeObject<DiscordGuild>(payload);
                        this.InternalGuilds.Add(guild);
                        break;
                    }
                case DispatchType.MessageCreate:
                    {
                        var msg = JsonConvert.DeserializeObject<DiscordMessage>(payload);
                        //if(msg.Content.StartsWith(prefix))
                        //Command start here
                        //Message Create Event
                        break;
                    }
                default:
                    {
                        //Unkown Dispatch event
                        break;
                    }
            }
        }
    }
}
