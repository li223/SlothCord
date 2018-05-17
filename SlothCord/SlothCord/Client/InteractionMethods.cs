using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SlothCord.Client
{
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

            var msg = new HttpRequestMessage(new HttpMethod("PATCH"), new Uri($"{_baseAddress}/channels/{channel_id}/messages/{message_id}"))
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
            var jsondata = JsonConvert.SerializeObject(new MemberModifyPayload()
            {
                Nickname = nickname,
                Roles = roles.Select(x => x.Id),
                IsMute = is_muted,
                IsDeaf = is_deaf,
                ChannelId = channel_id
            });
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), new Uri($"{_baseAddress}/guilds/{guild_id}/members/{member_id}"))
            {
                Content = new StringContent(jsondata, Encoding.UTF8, "application/json")
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
