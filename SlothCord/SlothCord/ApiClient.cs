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

namespace SlothCord.Objects
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

    public class GuildMethods : ApiBase
    {
        internal async Task DeleteMemberAsync(ulong guild_id, ulong member_id)
        {
            var msg = new HttpRequestMessage(HttpMethod.Delete, new Uri($"{_baseAddress}/guilds/{guild_id}/members/{member_id}"));
            var response = await _httpClient.SendAsync(msg).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                    await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg).ConfigureAwait(false);
        }

        internal async Task DeleteGuildBanAsync(ulong guild_id, ulong user_id)
        {
            var msg = new HttpRequestMessage(HttpMethod.Delete, new Uri($"{_baseAddress}/guilds/{guild_id}/bans/{user_id}"));
            var response = await _httpClient.SendAsync(msg).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                    await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg).ConfigureAwait(false);
        }

        internal async Task LeaveGuildAsync(ulong guild_id)
        {
            var msg = new HttpRequestMessage(HttpMethod.Delete, new Uri($"{_baseAddress}/users/@me/guilds/{guild_id}"));
            var response = await _httpClient.SendAsync(msg).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                    await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg).ConfigureAwait(false);
        }

        internal async Task CreateBanAsync(ulong guild_id, ulong member_id, int clear_days = 0, string reason = null)
        {
            if (clear_days < 0 || clear_days > 7)
                throw new ArgumentException("Clear days must be between 0 - 7");
            var query = $"{_baseAddress}/guilds/{guild_id}/bans/{member_id}?delete-message-days={clear_days}";
            if (reason != null)
                query += $"&reason={reason}";
            var request = new HttpRequestMessage(HttpMethod.Put, new Uri(query));
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                    await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), request).ConfigureAwait(false);
        }

        internal async Task<IReadOnlyList<DiscordGuildMember>> ListGuildMembersAsync(ulong guild_id, int limit = 100, ulong? around = null)
        {
            var requeststring = $"{_baseAddress}/guilds/{guild_id}/members?limit={limit}";
            if (around != null)
                requeststring += $"&around={around}";
            var msg = new HttpRequestMessage(HttpMethod.Get, new Uri(requeststring));
            var response = await _httpClient.SendAsync(msg).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var members = JsonConvert.DeserializeObject<List<DiscordGuildMember>>(content);
                for (var i = 0; i < members.Count(); i++) members[i].GuildId = guild_id;
                return members as IReadOnlyList<DiscordGuildMember>;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                    return JsonConvert.DeserializeObject<IReadOnlyList<DiscordGuildMember>>(await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg).ConfigureAwait(false));
                else return null;
            }
        }

        internal async Task<DiscordGuildMember> ListGuildMemberAsync(ulong guild_id, ulong member_id)
        {
            var msg = new HttpRequestMessage(HttpMethod.Get, new Uri($"{_baseAddress}/guilds/{guild_id}/members/{member_id}"));
            var response = await _httpClient.SendAsync(msg).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var member = JsonConvert.DeserializeObject<DiscordGuildMember>(content);
                member.GuildId = guild_id;
                return member;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                    return JsonConvert.DeserializeObject<DiscordGuildMember>(await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg).ConfigureAwait(false));
                else return null;
            }
        }

        internal async Task<DiscordChannel> ListGuildChannelAsync(ulong guild_id, ulong channel_id)
        {
            var msg = new HttpRequestMessage(HttpMethod.Get, new Uri($"{_baseAddress}/channels/{channel_id}"));
            var response = await _httpClient.SendAsync(msg).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var channel = JsonConvert.DeserializeObject<DiscordChannel>(content);
                if (channel.GuildId == null || channel.GuildId != guild_id) return null;
                else return channel;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                    return JsonConvert.DeserializeObject<DiscordChannel>(await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg).ConfigureAwait(false));
                else return null;
            }
        }

        internal async Task<AuditLogData> ListAuditLogsAsync(ulong guild_id, ulong? user_id = null, AuditActionType? action_type = null, ulong? before = null, int? limit = null)
        {

            #region kms.jpg
            bool addedextra = false;
            var query = $"{_baseAddress}/guilds/{guild_id}/audit-logs";
            if (user_id != null) query += $"?user_id={user_id}";
            if (action_type != null)
            {
                if (!addedextra) query += $"?action_type={(int)action_type}";
                else query += $"&action_type={(int)action_type}";
            }
            if (before != null)
            {
                if (!addedextra) query += $"?before={before}";
                else query += $"&before={before}";
            }
            if (limit != null)
            {
                if (!addedextra) query += $"?limit={limit}";
                else query += $"&limit={limit}";
            }
            #endregion
            var msg = new HttpRequestMessage(HttpMethod.Get, new Uri(query));
            var response = await _httpClient.SendAsync(msg);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<AuditLogData>(content);
            else
            {
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                    return JsonConvert.DeserializeObject<AuditLogData>(await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg).ConfigureAwait(false));
                else return null;
            }
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
                else { return null; }
            }
            else return null;
        }

        internal async Task<DiscordInvite> GetDiscordInviteAsync(string code, int? with_counts = null)
        {
            var query = $"{_baseAddress}/invites/{code}";
            if (with_counts != null)
                query += $"/with_counts/{with_counts}";
            var msg = new HttpRequestMessage(HttpMethod.Get, new Uri(query));
            var response = await _httpClient.SendAsync(msg);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString())) { return JsonConvert.DeserializeObject<DiscordInvite>(await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg)); }
                else { return null; }
            else return JsonConvert.DeserializeObject<DiscordInvite>(content);
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
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString())) { await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg).ConfigureAwait(false); }
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