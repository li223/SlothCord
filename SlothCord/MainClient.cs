using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net;

namespace SlothCord
{
    public class DiscordClient : ApiBase
    {
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

        private int _sequence = 0;
        private int _guildsToDownload = 0;
        private int _downloadedGuilds = 0;
        private string _sessionId = "";
        private int _heartbeatInterval = 0;

        internal List<DiscordGuild> AvailableGuilds = new List<DiscordGuild>();

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
            if (string.IsNullOrEmpty(this.Token))
                throw new ArgumentException("Token cannot be null");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"{TokenType} {this.Token}");
            this.Token = this.Token;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "DiscordBot ($https://fake.com/fake, $1.0.0)");
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"{_baseAddress}/gateway/bot"));
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var jobj = JsonConvert.DeserializeObject<HttpPayload>(content);
                WebSocketClient = new WebSocket(jobj.WSUrl);
                WebSocketClient.MessageReceived += WebSocketClient_MessageReceived;
                WebSocketClient.Closed += WebSocketClient_Closed;
                //await WebSocketClient.OpenAsync();
                WebSocketClient.Open();
            }
            else
            {
                ClientErrored?.Invoke(this, new ClientErroredArgs()
                {
                    Message = $"Server responded with: {JObject.Parse(content).SelectToken("message")}",
                    Source = "HttpClient"
                });
            }
        }

        private void WebSocketClient_Closed(object sender, EventArgs e) => SocketClosed?.Invoke(this, e);

        private void WebSocketClient_Error(object sender, ErrorEventArgs e) =>
            SocketErrored?.Invoke(this, new SocketDataArgs()
            {
                Data = e.Exception.Message
            });

        private async void WebSocketClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var data = JsonConvert.DeserializeObject<GatewayPayload>(e.Message);
            if (!string.IsNullOrEmpty(data.Sequence.ToString()))
                _sequence = (int)data.Sequence;
            switch (data.Code)
            {
                case OPCode.Hello:
                    {
                        if (_sessionId == "")
                        {
                            data.EventPayload = JsonConvert.DeserializeObject<GatewayHello>(data.EventPayload.ToString());
                            _heartbeatInterval = (data.EventPayload as GatewayHello).HeartbeatInterval;
                            var hbt = new Task(SendHeartbeats, TaskCreationOptions.LongRunning);
                            hbt.Start();
                            await SendIdentifyAsync();
                        }
                        break;
                    }
                case OPCode.Dispatch:
                    {
                        switch (data.EventName)
                        {
                            case "READY":
                                {
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
                            case "GUILD_CREATE":
                                {
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
                                    break;
                                }
                            case "PRESENCE_UPDATE":
                                {
                                    var pl = JsonConvert.DeserializeObject<PresencePayload>(data.EventPayload.ToString());
                                    var guild = this.Guilds.First(x => x.Id == pl.GuildId);
                                    var member = guild.Members.First(x => x.UserData.Id == pl.User.Id);
                                    var args = new PresenceUpdateArgs() { MemberBefore = member };
                                    member.UserData = pl.User;
                                    member.Nickname = pl.Nickname;
                                    member.UserData.Status = pl.Status;
                                    member.Roles = pl.RoleIds.Select(x => guild.Roles.First(a => a.Id == x)) as IReadOnlyList<DiscordRole>;
                                    member.UserData.Game = pl.Game;
                                    args.MemberAfter = member;
                                    PresenceUpdated?.Invoke(this, args);
                                    break;
                                }
                            case "MESSAGE_CREATE":
                                {
                                    var msg = JsonConvert.DeserializeObject<DiscordMessage>(data.EventPayload.ToString());
                                    MessageCreated?.Invoke(this, msg);
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
                        break;
                    }
                case OPCode.HeartbeatAck:
                    {
                        Heartbeated?.Invoke(this, new OnHeartbeatArgs() { Message = "WebSocket Heartbeat Ack" });
                        break;
                    }
                case OPCode.Reconnect:
                    {
                        await WebSocketClient.CloseAsync();
                        await WebSocketClient.OpenAsync();
                        var tsk = new Task(SendHeartbeats);
                        tsk.Start();
                        break;
                    }
                case OPCode.InvalidSession:
                    {
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
                        UnknownEvent?.Invoke(this, new UnkownEventArgs()
                        {
                            EventName = data.EventName,
                            OPCode = ((int)data.Code)
                        });
                    }
                    break;
            }
        }

        internal Task SendIdentifyAsync()
        {
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
        internal async Task CreateBanAsync(ulong guild_id, ulong member_id, int clear_days = 0, string reason = null)
        {
            if (clear_days < 0 || clear_days > 7)
                throw new ArgumentException("Clear days must be between 0 - 7");
            var query = $"{_baseAddress}/guilds/{guild_id}/bans/{member_id}?delete-message-days={clear_days}";
            if (reason != null)
                query += $"&reason={reason}";
            var request = new HttpRequestMessage(HttpMethod.Put, new Uri(query));
            var response = await _httpClient.SendAsync(request);
        }
    }

    public class ChannelMethods : ApiBase
    { 
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
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<IEnumerable<DiscordMessage>>(await response.Content.ReadAsStringAsync());
            else return null;
        }

        internal async Task<DiscordMessage> GetSingleMessageAsync(ulong channel_id, ulong message_id)
        {
            var response = await _httpClient.GetAsync(new Uri($"{_baseAddress}/channels/{channel_id}/messages/{message_id}"));
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordMessage>(await response.Content.ReadAsStringAsync());
            else return null;
        }

        internal async Task<DiscordChannel> DeleteGuildChannelAsync(ulong channel_id)
        {
            var response = await _httpClient.DeleteAsync(new Uri($"{_baseAddress}/channels/{channel_id}"));
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordChannel>(await response.Content.ReadAsStringAsync());
            else return null;
        }

        internal async Task<DiscordMessage> CreateMessageAsync(ulong channel_id, string message = null, bool is_tts = false, DiscordEmbed embed = null)
        {
            if (message.Length > 2000)
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
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordMessage>(await response.Content.ReadAsStringAsync());
            else return null;
        }
    }
}