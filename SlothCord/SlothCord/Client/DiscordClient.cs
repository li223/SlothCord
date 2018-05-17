using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SlothCord.Objects.ClientEntites;
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

       /* /// <summary>
        /// Command service used for bot commands
        /// </summary>
        public CommandService Commands { get; set; }*/

        /// <summary>
        /// The current client as a user
        /// </summary>
        public DiscordUser CurrentUser { get; internal set; }

        /// <summary>
        /// The current bot application
        /// </summary>
        public DiscordApplication CurrentApplication { get; internal set; }

        public async Task ConnectAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Token)) throw new ArgumentNullException("You must supply a valid token");
            if (TokenType != TokenType.Bot) throw new ArgumentException("Only Bot tokens are currently supported");
            var response = await _httpClient.GetAsync($"{_baseAddress}/gateway/bot").ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var data = JObject.Parse(content);
            if (!response.IsSuccessStatusCode) throw new Exception($"Gateway returned: {data["message"].ToString()}");
            var shards = data["shards"].ToString();
            var url = data["url"].ToString();
            WebSocketClient = new WebSocket(url);
#if NETCORE
            await WebSocketClient.OpenAsync().ConfigureAwait(false);
#else
            WebSocketClient.Open();
#endif

        }
    }
}
