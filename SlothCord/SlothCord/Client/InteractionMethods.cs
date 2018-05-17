using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using SlothCord.Objects;

namespace SlothCord
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

    public class ChannelMethods : ApiBase
    {
        internal async Task<DiscordMessage> CreateMessageWithFile(ulong channel_id, string file_path, string message = null)
        {
            if (message?.Length > 2000)
                throw new ArgumentException("Message cannot exceed 2000 characters");
            var data = File.ReadAllBytes(file_path);
            var jsondata = JsonConvert.SerializeObject(new MessageCreatePayload()
            {
                FileData = data
            });
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"{_baseAddress}/channels/{channel_id}/messages"))
            {
                Content = new StringContent(jsondata, Encoding.UTF8)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordMessage>(content);
            else if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString())) return JsonConvert.DeserializeObject<DiscordMessage>(await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), request).ConfigureAwait(false));
            else return null;
        }

        internal async Task DeleteChannelMessageAsync(ulong channel_id, ulong message_id)
        {
            var msg = new HttpRequestMessage(HttpMethod.Delete, new Uri($"{_baseAddress}/channels/{channel_id}/messages/{message_id}"));
            var response = await _httpClient.SendAsync(msg).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                    await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg).ConfigureAwait(false);
        }

        internal async Task<DiscordInvite> DeleteDiscordInviteAsync(string code, int? with_counts = null)
        {
            var msg = new HttpRequestMessage(HttpMethod.Delete, new Uri($"{_baseAddress}/invites/{code}"));
            var response = await _httpClient.SendAsync(msg).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString())) { return JsonConvert.DeserializeObject<DiscordInvite>(await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg).ConfigureAwait(false)); }
                else return null;
            }
            else return null;
        }

        internal async Task<IReadOnlyList<DiscordInvite>> GetChannelInvitesAsync(ulong channel_id)
        {
            var query = $"{_baseAddress}/channels/{channel_id}/invites";
            var msg = new HttpRequestMessage(HttpMethod.Get, new Uri(query));
            var response = await _httpClient.SendAsync(msg);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString())) { return JsonConvert.DeserializeObject<IReadOnlyList<DiscordInvite>>(await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg)); }
                else return null;
            }
            else return JsonConvert.DeserializeObject<IReadOnlyList<DiscordInvite>>(content);
        }

        internal async Task BulkDeleteGuildMessagesAsync(ulong? guild_id, ulong channel_id, IReadOnlyList<ulong> message_ids)
        {
            if (guild_id == null) return;
            var ids = new BulkDeletePayload()
            {
                Messages = message_ids.ToArray()
            };

            var msg = new HttpRequestMessage(HttpMethod.Post, new Uri($"{_baseAddress}/channels{channel_id}/messages/bulk-delete"))
            {
                Content = new StringContent(JsonConvert.SerializeObject(ids))
            };

            var response = await _httpClient.SendAsync(msg).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                    await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg).ConfigureAwait(false);

        }

        internal async Task<IReadOnlyList<DiscordMessage>> GetMultipleMessagesAsync(ulong channel_id, int limit = 100, ulong? around = null, ulong? after = null, ulong? before = null)
        {
            var requeststring = $"{_baseAddress}/channels/{channel_id}/messages?limit={limit}";

            if (around != null)
                requeststring += $"&around={around}";
            if (before != null)
                requeststring += $"&before={before}";
            if (after != null)
                requeststring += $"&after={after}";

            var response = await _httpClient.GetAsync(new Uri(requeststring)).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<IReadOnlyList<DiscordMessage>>(content);
            else return null;
        }

        internal async Task<DiscordMessage> GetSingleMessageAsync(ulong channel_id, ulong message_id)
        {
            var response = await _httpClient.GetAsync(new Uri($"{_baseAddress}/channels/{channel_id}/messages/{message_id}")).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordMessage>(content);
            else return null;
        }

        internal async Task<DiscordChannel> DeleteChannelAsync(ulong channel_id)
        {
            var response = await _httpClient.DeleteAsync(new Uri($"{_baseAddress}/channels/{channel_id}")).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordChannel>(content);
            else return null;
        }

        internal async Task<DiscordMessage> CreateMessageAsync(ulong channel_id, string message = null, bool is_tts = false, DiscordEmbed embed = null)
        {
            if (message?.Length > 2000)
                throw new ArgumentException("Message cannot exceed 2000 characters");

            if (string.IsNullOrEmpty(message) && embed == null)
                throw new ArgumentNullException("Cannot send an empty message");

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
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordMessage>(content);
            else if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString())) return JsonConvert.DeserializeObject<DiscordMessage>(await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), request).ConfigureAwait(false));
            else return null;

        }

        internal async Task<DiscordChannel> ModifyGuildChannelAsync(ulong channel_id, string name = null, int? position = null, string topic = null, bool? nsfw = null, int? bitrate = null, int? user_limit = null, IReadOnlyList<ChannelOverwrite> permission_overwrites = null, ulong? parent_id = null)
        {
            var msg = new HttpRequestMessage(new HttpMethod("PATCH"), new Uri($"{_baseAddress}/channels/{channel_id}"))
            {
                Content = new StringContent(JsonConvert.SerializeObject(new ChannelModifyPayload()
                {
                    Name = name,
                    Position = position,
                    Topic = topic,
                    IsNsfw = nsfw,
                    Bitrate = bitrate,
                    UserLimit = user_limit,
                    PermissionOverwrites = permission_overwrites,
                    ParentId = parent_id
                }))
            };
            var response = await _httpClient.SendAsync(msg).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordChannel>(content);
            else if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                return JsonConvert.DeserializeObject<DiscordChannel>(await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg).ConfigureAwait(false) );
            else return null;
        }
    }
}
