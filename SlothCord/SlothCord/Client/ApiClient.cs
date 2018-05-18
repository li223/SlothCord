using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using WebSocket4Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using SlothCord.Objects;

namespace SlothCord
{
    public class ApiBase
    {
        protected internal static HttpClient _httpClient = new HttpClient();

        protected internal static WebSocket WebSocketClient { get; set; }

        protected internal static Uri _baseAddress = new Uri("https://discordapp.com/api/v7");

        protected internal async Task<string> RetryAsync(int retry_in, HttpRequestMessage msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Gateway Ratelimit Reached, waiting {retry_in}ms");
            Console.ForegroundColor = ConsoleColor.White;
            await Task.Delay(retry_in).ConfigureAwait(false);
            var response = await _httpClient.SendAsync(msg).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return content;
        }

        internal async Task<DiscordApplication> GetCurrentApplicationAsync()
        {
            var msg = new HttpRequestMessage(HttpMethod.Get, new Uri($"{_baseAddress}/oauth2/applications/@me"));
            var response = await _httpClient.SendAsync(msg).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordApplication>(content);
            else
            {
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                    return JsonConvert.DeserializeObject<DiscordApplication>(await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg).ConfigureAwait(false));
                else throw new Exception($"Returned Message: {content}");
            }
        }
    }
}
