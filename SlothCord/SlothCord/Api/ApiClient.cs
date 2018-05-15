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

        protected internal async Task HandleLimitAsync(HttpRequestMessage msg, KeyValuePair<int, int> requests, int limit)
        {

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

    public class MessageMethods : ApiBase
    {
        internal async Task<DiscordMessage> EditDiscordMessageAsync(ulong channel_id, ulong message_id, string content, DiscordEmbed embed)
        {
            if (embed == null && content == null) throw new ArgumentNullException("Cannot send empty message");

            var obj = new MessageUpdatePayload()
            {
                Content = content,
                Embed = embed
            };

            var msg = new HttpRequestMessage(HttpMethod.Put, new Uri($"{_baseAddress}/channels/{channel_id}/messages/{message_id}"))
            {
                Content = new StringContent(JsonConvert.SerializeObject(obj))
            };
            var response = await _httpClient.SendAsync(msg).ConfigureAwait(false);
            var rescont = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordMessage>(rescont);
            else
            {
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                    return JsonConvert.DeserializeObject<DiscordMessage>(await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg).ConfigureAwait(false));
                else throw new Exception($"Returned Message: {rescont}");
            }
        }

        internal async Task DeleteMessageAsync(ulong channel_id, ulong message_id)
        {
            var msg = new HttpRequestMessage(HttpMethod.Delete, new Uri($"{_baseAddress}/channels/{channel_id}/messages/{message_id}"));
            var response = await _httpClient.SendAsync(msg).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                    await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg).ConfigureAwait(false);
            }

        }
    }

    public class UserMethods : ApiBase
    {
        internal async Task<DiscordChannel> CreateUserDmChannelAsync(ulong user_id)
        {
            var response = await _httpClient.PostAsync($"{_baseAddress}/users/@me/channels?recipient_id={user_id}", null).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordChannel>(content);
            else return null;
        }
    }

    public class MemberMethods : ApiBase
    {
        internal async Task ModifyAsync(ulong guild_id, ulong member_id, string nickname, IReadOnlyList<DiscordRole> roles, bool? is_muted, bool? is_deaf, ulong? channel_id)
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
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                    await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), request).ConfigureAwait(false);
        }

        internal async Task<DiscordChannel> CreateUserDmChannelAsync(ulong user_id)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"{_baseAddress}/users/@me/channels?recipient_id={user_id}"));
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordChannel>(content);
            else if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                return JsonConvert.DeserializeObject<DiscordChannel>(await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), request).ConfigureAwait(false));
            else return null;
        }
    }
}