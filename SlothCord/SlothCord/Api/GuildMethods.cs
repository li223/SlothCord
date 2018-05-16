using Newtonsoft.Json;
using SlothCord.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SlothCord
{
    public class GuildMethods : ApiBase
    {
        internal async Task<IReadOnlyList<DiscordInvite>> GetGuildInvitesAsync(ulong guild_id)
        {
            var query = $"{_baseAddress}/guilds/{guild_id}/invites";
            var msg = new HttpRequestMessage(HttpMethod.Get, new Uri(query));
            var response = await _httpClient.SendAsync(msg);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString())) { return JsonConvert.DeserializeObject<IReadOnlyList<DiscordInvite>>(await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg)); }
                else { return null; }
            else return JsonConvert.DeserializeObject<IReadOnlyList<DiscordInvite>>(content);
        }

        internal async Task<GuildEmbed> ModifyGuildEmbedAsync(ulong guild_id, bool enabled, ulong channel_id)
        {
            var msg = new HttpRequestMessage(new HttpMethod("PATCH"), new Uri($"{_baseAddress}/guilds/{guild_id}/embed"))
            {
                Content = new StringContent(JsonConvert.SerializeObject(new GuildEmbed()
                {
                    IsEnabled = enabled,
                    ChannelId = channel_id
                }))
            };
            var response = await _httpClient.SendAsync(msg).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<GuildEmbed>(content);
            else
            {
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                    return JsonConvert.DeserializeObject<GuildEmbed>(await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg).ConfigureAwait(false));
                else return null;
            }
        }

        internal async Task<GuildEmbed> GetGuildEmbedAsync(ulong guild_id)
        {
            var msg = new HttpRequestMessage(HttpMethod.Get, new Uri($"{_baseAddress}/guilds/{guild_id}/embed"));
            var response = await _httpClient.SendAsync(msg).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<GuildEmbed>(content);
            else
            {
                if (!string.IsNullOrWhiteSpace(response.Headers.RetryAfter?.ToString()))
                    return JsonConvert.DeserializeObject<GuildEmbed>(await RetryAsync(int.Parse(response.Headers.RetryAfter.ToString()), msg).ConfigureAwait(false));
                else return null;
            }
        }

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
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), new Uri(query));
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
            bool addextra = false;
            var query = $"{_baseAddress}/guilds/{guild_id}/audit-logs";
            if (user_id != null) query += $"?user_id={user_id}";
            if (action_type != null)
            {
                if (!addextra)
                {
                    query += $"?action_type={(int)action_type}";
                    addextra = true;
                }
                else query += $"&action_type={(int)action_type}";
            }
            if (before != null)
            {
                if (!addextra)
                {
                    query += $"?before={before}";
                    addextra = true;
                }
                else query += $"&before={before}";
            }
            if (limit != null)
            {
                if (!addextra)
                {
                    query += $"?limit={limit}";
                    addextra = true;
                }
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
}
