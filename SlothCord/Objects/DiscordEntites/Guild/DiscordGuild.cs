﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlothCord.Objects
{
    public sealed class DiscordGuild : GuildMethods
    {
        public async Task<IEnumerable<GuildBan>> GetBansAsync()
            => await base.GetGuildBansAsync(this.Id).ConfigureAwait(false);

        public async Task<IEnumerable<DiscordGuildInvite>> GetInvitesAsync()
            => await base.GetGuildInvitesAsync(this.Id).ConfigureAwait(false);

        public async Task<GuildEmbed?> GetEmbedAsync()
            => await base.GetGuildEmbedAsync(this.Id);

        public async Task<GuildEmbed?> ModifyEmbedAsync(bool enabled = true, ulong channel_id = 0)
            => await base.ModifyGuildEmbedAsync(this.Id, enabled, channel_id);

        public async Task KickMemberAsync(DiscordGuildMember member)
            => await base.DeleteMemberAsync(this.Id, member.UserData.Id).ConfigureAwait(false);

        public async Task KickMemberAsync(ulong id)
            => await base.DeleteMemberAsync(this.Id, id).ConfigureAwait(false);

        public async Task RemoveBanAsync(DiscordUser user)
            => await base.DeleteGuildBanAsync(this.Id, user.Id).ConfigureAwait(false);

        public async Task RemoveBanAsync(ulong id)
            => await base.DeleteGuildBanAsync(this.Id, id).ConfigureAwait(false);

        public DiscordGuildMember GetMember(ulong id)
            => this.Members?.FirstOrDefault(x => x.UserData.Id == id);

        public DiscordGuildChannel GetChannel(ulong id)
            => this.Channels?.FirstOrDefault(x => x.Id == id);

        public DiscordGuildRole? GetRole(ulong id)
            => this.Roles?.FirstOrDefault(x => x.Value.Id == id);

        public async Task<GuildAuditLogData?> GetAuditLogsAsync(ulong? user_id = null, AuditActionType? action_type = null, ulong? before = null, int? limit = null)
            => await base.ListAuditLogsAsync(this.Id, user_id, action_type, before, limit).ConfigureAwait(false);

        public async Task LeaveAsync()
            => await base.LeaveGuildAsync(this.Id).ConfigureAwait(false);

        public async Task<DiscordChannel> GetChannelAsync(ulong channel_id)
            => await base.ListGuildChannelAsync(this.Id, channel_id).ConfigureAwait(false);

        public async Task<DiscordGuildMember> GetMemberAsync(ulong user_id)
        {
            var member = await base.ListGuildMemberAsync(this.Id, user_id).ConfigureAwait(false);
            member.Guild = this;
            return member;
        }

        public async Task<IEnumerable<DiscordGuildMember>> GetMembersAsync(int limit = 100, ulong? around = null)
        {
            var members = await base.ListGuildMembersAsync(this.Id, limit, around).ConfigureAwait(false);
            foreach (var member in members)
            {
                member.Guild = this;
                member.Roles = this.Roles.Where(x => member.RoleIds.Any(a => a == x.Value.Id)) as IReadOnlyList<DiscordGuildRole?>;
            }
            return members;
        }

        public async Task BanMemberAsync(DiscordGuildMember member, int clear_days = 0, string reason = null)
            => await base.CreateBanAsync(this.Id, member.UserData.Id, clear_days, reason).ConfigureAwait(false);

        public async Task BanMemberAsync(ulong id, int clear_days = 0, string reason = null)
            => await base.CreateBanAsync(this.Id, id, clear_days, reason).ConfigureAwait(false);

        public async Task BanB1nzyAsync()
            => await base.CreateBanAsync(this.Id, 80351110224678912, 0, "B1nzy got ratelimited").ConfigureAwait(false);
        
        [JsonIgnore]
        public string IconUrl { get => $"https://cdn.discordapp.com/icons/{this.Id}/{this.Icon}.png"; }

        [JsonIgnore]
        public DiscordGuildMember Owner { get => this.Members.FirstOrDefault(x => x.UserData.Id == this.OwnerId); }

        [JsonIgnore]
        public DateTimeOffset CreatedAt { get; internal set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("channels")]
        public IReadOnlyList<DiscordGuildChannel> Channels { get; private set; }

        [JsonProperty("large")]
        public bool IsLarge { get; private set; }

        [JsonProperty("voice_states")]
        public IReadOnlyList<VoiceState?> VoiceStates { get; private set; }

        [JsonProperty("system_channel_id")]
        public ulong? DefaultChannelId { get; private set; }

        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("application_id")]
        public ulong? ApplicationId { get; private set; }

        [JsonProperty("icon")]
        public string Icon { get; private set; }

        [JsonProperty("splash")]
        public string SplashUrl { get; private set; }

        [JsonProperty("OwnerId")]
        public ulong? OwnerId { get; private set; }

        [JsonProperty("region")]
        public string Region { get; private set; }

        [JsonProperty("afk_channel_id")]
        public ulong? AfkChannelId { get; private set; }

        [JsonProperty("afk_timeout")]
        public int? AfkTimeout { get; private set; }

        [JsonProperty("embed_enabled")]
        public bool? EmbedsEnabled { get; private set; }

        [JsonProperty("embed_channel_id")]
        public ulong? EmbedChannelId { get; private set; }

        [JsonProperty("verification_level")]
        public VerificationLevel? VerificationLevel { get; private set; }

        [JsonProperty("default_message_notifications")]
        public NotificationLevel? DefaultMessageNotifications { get; private set; }

        [JsonProperty("explicit_content_filter")]
        public ExplicitContentFilterLevel? ExplicitContentFilter { get; private set; }

        [JsonProperty("mfa_level")]
        public MFALevel? MfaLevel { get; private set; }

        [JsonProperty("widget_enabled")]
        public bool? WidgetEnabled { get; private set; }

        [JsonProperty("widget_channel_id")]
        public ulong? WidgetChannelId { get; private set; }

        [JsonProperty("roles")]
        public IReadOnlyList<DiscordGuildRole?> Roles { get; private set; }

        [JsonProperty("emojis")]
        public IReadOnlyList<DiscordGuildEmoji?> Emojis { get; private set; }

        [JsonProperty("features")]
        public IReadOnlyList<string> Features { get; private set; }

        [JsonProperty("presences")]
        public IReadOnlyList<DiscordPresence?> Presences { get; private set; }

        [JsonProperty("members")]
        public IReadOnlyList<DiscordGuildMember> Members { get; private set; }

        [JsonProperty("unavailable")]
        public bool IsUnavailable { get; private set; }
    }
}