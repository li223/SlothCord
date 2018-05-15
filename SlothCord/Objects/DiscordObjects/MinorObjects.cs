using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace SlothCord.Objects
{
    public sealed class DiscordPresence
    {
        [JsonProperty("game")]
        public DiscordGame Game { get; private set; }

        [JsonProperty("status")]
        public StatusType? Status { get; private set; }

        [JsonProperty("since")]
        public long? Since { get; private set; }

        [JsonProperty("afk")]
        public bool? AFK { get; private set; } = false;
    }

    public sealed class DiscordVoiceState { }

    public sealed class DiscordGame
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public PlayingType Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public struct DiscordApplication
    {
        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("bot_public")]
        public bool IsPublicBot { get; private set; }

        [JsonProperty("bot_require_code_grant")]
        public bool RequiresCodeGrant { get; private set; }

        [JsonProperty("owner")]
        public DiscordUser Owner { get; private set; }

        [JsonProperty("description")]
        public string Description { get; private set; }

        [JsonProperty("icon")]
        private string Icon { get; set; }

        [JsonIgnore]
        public string IconUrl { get => $"https://cdn.discordapp.com/icons/{this.Id}/{this.Icon}.png"; }
    }

    public sealed class DiscordActivity
    {
        [JsonProperty("url")]
        public string Url { get; private set; }

        [JsonProperty("type")]
        public ActivityType Type { get; set; }

        [JsonProperty("party_id")]
        public ulong? PartyId { get; private set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("application_id")]
        public long? ApplicationId { get; private set; }
        
        [JsonProperty("state")]
        public string State { get; private set; }

        [JsonProperty("details")]
        public string Details { get; private set; }

        [JsonProperty("timestamps")]
        public ActivityTimestamps Timestamps { get; private set; }

        [JsonProperty("party")]
        public ActivityParty Party { get; private set; }

        [JsonProperty("assets")]
        public ActivityAssets Assets { get; private set; }

        [JsonProperty("secrets")]
        public ActivitySecrets Secrets { get; private set; }

        [JsonProperty("instance")]
        public bool? InGame { get; private set; }

        [JsonProperty("flags")]
        public int? Flags { get; private set; }
    }

    public sealed class DiscordReaction { }

    public sealed class DiscordAttachment { }

    public sealed class AuditLogData
    {
        [JsonProperty("webhooks")]
        public IReadOnlyList<Webhook> Webhooks { get; private set; }

        [JsonProperty("users")]
        public IReadOnlyList<DiscordUser> Users { get; private set; }

        [JsonProperty("audit_log_entries")]
        public IReadOnlyList<AuditEntryObject> Entries { get; private set; }
    }

    public sealed class DiscordEmoji
    {
        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("roles")]
        public IReadOnlyList<ulong> RoleIds { get; private set; }

        [JsonProperty("user")]
        public DiscordUser Creator { get; private set; } = null;

        [JsonProperty("require_colons")]
        public bool? RequiresColons { get; private set; }

        [JsonProperty("managed")]
        public bool? IsManaged { get; private set; }

        [JsonProperty("animated")]
        public bool? IsAnimated { get; private set; }
    }

    public sealed class DiscordRole
    {
        [JsonProperty("position")]
        public int Postition { get; private set; }

        [JsonProperty("permissions")]
        public Permissions Permissions { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("Mentionable")]
        public bool Mentionable { get; private set; }

        [JsonProperty("managed")]
        public bool Managed { get; private set; }

        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("hoist")]
        public bool IsHoisted { get; private set; }

        [JsonProperty("color")]
        private int IntColorValue { get; set; }

        [JsonIgnore]
        public string Mention { get => $"<@&{this.Id}>"; }
    }

    public sealed class ChannelOverwrite { }

    public sealed class DiscordInvite
    {
        [JsonProperty("code")]
        public string Code { get; private set; }

        [JsonProperty("guild")]
        public DiscordGuild Guild { get; private set; }

        [JsonProperty("channel")]
        public DiscordChannel Channel { get; private set; }

        [JsonProperty("inviter")]
        public DiscordUser Inviter { get; private set; }

        [JsonProperty("uses", NullValueHandling = NullValueHandling.Ignore)]
        public int? Uses { get; private set; }

        [JsonProperty("max_uses", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxUses { get; private set; }

        [JsonProperty("temporary", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsTemporary { get; private set; }

        [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CreatedAt { get; private set; }

        [JsonProperty("revoked", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsRevoked { get; private set; }
    }

    public struct Webhook { }

    public struct Category
    {
        [JsonProperty("permission_overwrites", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<ChannelOverwrite> Overwrite { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ParentId { get; private set; }

        [JsonProperty("position")]
        public int Position { get; private set; }

        [JsonProperty("guild_id")]
        public ulong GuildId { get; private set; }

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
        public IReadOnlyList<AuditChange> Changes { get; private set; }

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

    public struct ActivitySecrets
    {
        [JsonProperty("join")]
        public string Join { get; private set; }

        [JsonProperty("spectate")]
        public string Spectate { get; private set; }

        [JsonProperty("match")]
        public string Match { get; private set; }
    }

    public struct ActivityAssets
    {
        [JsonProperty("large_image")]
        public string LargeImage { get; private set; }

        [JsonProperty("large_text")]
        public string LargeText { get; private set; }

        [JsonProperty("small_image")]
        public string SmallImage { get; private set; }

        [JsonProperty("small_text")]
        public string SmallText { get; private set; }
    }

    public struct ActivityParty
    {
        [JsonProperty("id")]
        public string Id { get; private set; }

        [JsonProperty("size")]
        public IReadOnlyList<int> Size { get; private set; }
    }

    public struct ActivityTimestamps
    {
        [JsonProperty("start")]
        public ulong? Start { get; private set; }

        [JsonProperty("end")]
        public ulong? End { get; private set; }
    }

    //Tbh I have no goddamn clue
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
        public IReadOnlyList<DiscordRole> RolesAdded { get; private set; }

        [JsonProperty("$remove")]
        public IReadOnlyList<DiscordRole> RolesRemoved { get; private set; }

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
        public IReadOnlyList<ChannelOverwrite> ChannelOverwrites { get; private set; }

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
        public int? RoleAllow { get; private set; }

        [JsonProperty("deny")]
        public int? RoleDeny { get; private set; }

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
