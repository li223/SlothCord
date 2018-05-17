using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlothCord.Objects
{
    public sealed class DiscordGuild
    {
        public async Task<IReadOnlyList<DiscordInvite>> GetInvitesAsync()
            => await base.GetGuildInvitesAsync(this.Id).ConfigureAwait(false);

        public async Task<GuildEmbed> GetEmbedAsync()
            => await base.GetGuildEmbedAsync(this.Id);

        public async Task<GuildEmbed> ModifyEmbedAsync(bool enabled = true, ulong channel_id = 0)
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
            => this.Members.FirstOrDefault(x => x.UserData.Id == id);

        public DiscordChannel GetChannel(ulong id)
            => this.Channels.FirstOrDefault(x => x.Id == id);

        public DiscordRole GetRole(ulong id)
            => this.Roles.FirstOrDefault(x => x.Id == id);

        public async Task<AuditLogData> GetAuditLogsAsync(ulong? user_id = null, AuditActionType? action_type = null, ulong? before = null, int? limit = null)
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

        public async Task<IReadOnlyList<DiscordGuildMember>> GetMembersAsync(int limit = 100, ulong? around = null)
        {
            var members = await base.ListGuildMembersAsync(this.Id, limit, around).ConfigureAwait(false);
            foreach (var member in members)
                member.Guild = this;
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
        public DiscordGuildMember Owner { get => this.Members.First(x => x.UserData.Id == this.OwnerId); }

        [JsonIgnore]
        public DateTimeOffset CreatedAt { get; internal set; }
        
        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("channels")]
        public IReadOnlyList<DiscordGuildChannel> Channels { get; }

        [JsonProperty("large")]
        public bool IsLarge { get; }

        [JsonProperty("voice_states")]
        public IReadOnlyList<DiscordVoiceState> VoiceStates { get; }

        [JsonProperty("system_channel_id")]
        public ulong? DefaultChannelId { get; } = 0;

        [JsonProperty("id")]
        public ulong Id { get; }

        [JsonProperty("application_id")]
        public ulong? ApplicationId { get; }

        [JsonProperty("icon")]
        public string Icon { get; }

        [JsonProperty("splash")]
        public string SplashUrl { get; }

        [JsonProperty("OwnerId")]
        public ulong? OwnerId { get; }

        [JsonProperty("region")]
        public string Region { get; }

        [JsonProperty("afk_channel_id")]
        public ulong? AfkChannelId { get; }

        [JsonProperty("afk_timeout")]
        public int? AfkTimeout { get; }

        [JsonProperty("embed_enabled")]
        public bool? EmbedsEnabled { get; }

        [JsonProperty("embed_channel_id")]
        public ulong? EmbedChannelId { get; }

        [JsonProperty("verification_level")]
        public VerificationLevel? VerificationLevel { get; }

        [JsonProperty("default_message_notifications")]
        public NotificationLevel? DefaultMessageNotifications { get; }

        [JsonProperty("explicit_content_filter")]
        public ExplicitContentFilterLevel? ExplicitContentFilter { get; }

        [JsonProperty("mfa_level")]
        public MFALevel? MfaLevel { get; }

        [JsonProperty("widget_enabled")]
        public bool? WidgetEnabled { get; }

        [JsonProperty("widget_channel_id")]
        public ulong? WidgetChannelId { get; }

        [JsonProperty("roles")]
        public IReadOnlyList<DiscordRole> Roles { get; }

        [JsonProperty("emojis")]
        public IReadOnlyList<DiscordEmoji> Emojis { get; }

        [JsonProperty("features")]
        public IReadOnlyList<string> Features { get; }

        [JsonProperty("presences")]
        public IReadOnlyList<DiscordPresence> Presences { get; }

        [JsonProperty("members")]
        public IReadOnlyList<DiscordGuildMember> Members { get; }

        [JsonProperty("unavailable")]
        public bool IsUnavailable { get; }
    }
}