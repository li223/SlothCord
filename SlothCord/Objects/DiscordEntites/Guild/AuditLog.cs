using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlothCord.Objects
{
    public struct GuildAuditLogData
    {
        [JsonProperty("webhooks")]
        public IEnumerable<Webhook> Webhooks { get; private set; }

        [JsonProperty("users")]
        public IEnumerable<DiscordUser> Users { get; private set; }

        [JsonProperty("audit_log_entries")]
        public IEnumerable<AuditEntryObject> Entries { get; private set; }
    }

    public struct AuditEntryObject
    {
        [JsonIgnore]
        public DateTimeOffset CreatedAt
        {
            get
            {
                var bin = this.Id.ToString("2");
                var sb = new StringBuilder();
                bin.Split().Take(64).Select(x => sb.Append(x));
                var de = (int.Parse(sb.ToString())) + 1420070400000;
                return DateTimeOffset.FromUnixTimeMilliseconds(de);
            }
        }

        [JsonProperty("target_id")]
        public ulong TargetType { get; private set; }

        [JsonProperty("changes", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<AuditChange> Changes { get; private set; }

        [JsonProperty("user_id")]
        public ulong UserResponsibleId { get; private set; }

        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("action_type")]
        public AuditActionType ActionType { get; private set; }

        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public Options? Options { get; private set; }

        [JsonProperty("reason")]
        public string Reason { get; private set; }
    }
    
    public struct Options
    {
        [JsonProperty("delete_member_days")]
        public int? DeleteMemberDays { get; private set; }

        [JsonProperty("members_removed")]
        public int? MembersRemoved { get; private set; }

        [JsonProperty("channel_id")]
        public string ChannelId { get; private set; }

        [JsonProperty("count")]
        public string Count { get; private set; }

        [JsonProperty("id")]
        public ulong? Id { get; private set; }

        [JsonProperty("type")]
        public string OverWrittenType { get; private set; }

        [JsonProperty("role_name")]
        public string RoleName { get; private set; }
    }

    public struct AuditChange
    {
        [JsonProperty("new_value")]
        public string NewValue { get; private set; }

        [JsonProperty("old_value")]
        public string OldValue { get; private set; }

        [JsonProperty("key")]
        public string TypeKey { get; private set; }
    }
    
    //Still not a clue
    public struct ChangeKey
    {
        [JsonProperty("guild")]
        public string GuildName { get; private set; }

        [JsonProperty("icon_hash")]
        public string IconHash { get; private set; }

        [JsonProperty("splash_hash")]
        public string SplashHash { get; private set; }

        [JsonProperty("owner_id")]
        public ulong? OwnerId { get; private set; }

        [JsonProperty("region")]
        public string Region { get; private set; }

        [JsonProperty("afk_channel_id")]
        public ulong? AfkChannelId { get; private set; }

        [JsonProperty("ask_timeout")]
        public int? AfkTimeout { get; private set; }

        [JsonProperty("mfa_level")]
        public MFALevel? MfaLevel { get; private set; }

        [JsonProperty("verification_level")]
        public VerificationLevel? VerificationLevel { get; private set; }

        [JsonProperty("explicit_content_filter")]
        public ExplicitContentFilterLevel? ExplicitContentFilterLevel { get; private set; }

        [JsonProperty("default_messages_notifications")]
        public NotificationLevel? NotificationLevel { get; private set; }

        [JsonProperty("vanity_url_code")]
        public string VanityUrlCode { get; private set; }

        [JsonProperty("$add")]
        public IEnumerable<DiscordGuildRole> RolesAdded { get; private set; }

        [JsonProperty("$remove")]
        public IEnumerable<DiscordGuildRole> RolesRemoved { get; private set; }

        [JsonProperty("prune_delete_days")]
        public int? PruneDeleteDays { get; private set; }

        [JsonProperty("widget_enabled")]
        public bool? WidgetEnabled { get; private set; }

        [JsonProperty("widget_channel_id")]
        public ulong? WdigetChannelId { get; private set; }

        [JsonProperty("position")]
        public int? Position { get; private set; }

        [JsonProperty("topic")]
        public string Topic { get; private set; }

        [JsonProperty("bitrate")]
        public int? Bitrate { get; private set; }
        
        [JsonProperty("permission_overwrites")]
        public IEnumerable<GuildChannelOverwrite> ChannelOverwrites { get; private set; }

        [JsonProperty("nsfw")]
        public bool? Nsfw { get; private set; }

        [JsonProperty("application_id")]
        public ulong? AppicationId { get; private set; }

        [JsonProperty("permissions")]
        public Permissions? Permissions { get; private set; }

        [JsonProperty("color")]
        public DiscordColor? Color { get; private set; }

        [JsonProperty("hoist")]
        public bool? IsHoisted { get; private set; }

        [JsonProperty("mentionable")]
        public bool? IsMentionable { get; private set; }

        [JsonProperty("allow")]
        public Permissions? RoleAllow { get; private set; }

        [JsonProperty("deny")]
        public Permissions? RoleDeny { get; private set; }

        [JsonProperty("code")]
        public string InviteCode { get; private set; }

        [JsonProperty("channel_id")]
        public ulong? InviteChannelId { get; private set; }

        [JsonProperty("inviter_id")]
        public ulong? InviterId { get; private set; }

        [JsonProperty("max_uses")]
        public int? InviteMaxUses { get; private set; }

        [JsonProperty("uses")]
        public int? InviteUses { get; private set; }

        [JsonProperty("max_age")]
        public ulong? MaxAgeUnix { get; private set; }

        [JsonProperty("temporary")]
        public bool? IsTemporary { get; private set; }

        [JsonProperty("mute")]
        public bool? IsMute { get; private set; }

        [JsonProperty("deaf")]
        public bool? IsDeaf { get; private set; }

        [JsonProperty("nick")]
        public string Nickname { get; private set; }

        [JsonProperty("avatar_hash")]
        public string AvatarHash { get; private set; }

        [JsonProperty("id")]
        public ulong? EntityId { get; private set; }

        [JsonProperty("type")]
        public string EntityType { get; private set; }
    }
}