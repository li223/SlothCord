using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SlothCord
{
    public sealed class DiscordUser
    {
        [JsonProperty("status")]
        public string Status { get; internal set; }

        [JsonProperty("game", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordGame Game { get; internal set; }

        [JsonProperty("verified")]
        public bool Verified { get; private set; }

        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("username")]
        public string Username { get; private set; }

        [JsonProperty("discriminator")]
        public int Discriminator { get; private set; }

        [JsonProperty("mfa_enbaled")]
        public bool MfaEnabled { get; private set; }

        [JsonProperty("bot")]
        public bool IsBot { get; private set; }

        [JsonProperty("email")]
        public string Email { get; private set; }

        [JsonProperty("avatar")]
        public string AvatarUrl { get; private set; }
    }
    public sealed class DiscordPresence
    {
        [JsonProperty("game")]
        public DiscordGame Game { get; private set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public StatusType Status { get; private set; }

        [JsonProperty("since", NullValueHandling = NullValueHandling.Ignore)]
        public long? Since { get; private set; }

        [JsonProperty("afk", NullValueHandling = NullValueHandling.Ignore)]
        public bool AFK { get; private set; } = false;
    }
    public sealed class DiscordVoiceState { }
    public sealed class DiscordGame
    {
        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("type")]
        public PlayingType Type { get; internal set; }
    }
    public sealed class DiscordMessage
    {
        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("channel_id")]
        public ulong ChannelId { get; private set; }

        [JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser Author { get; private set; }

        [JsonProperty("content")]
        public string Content { get; private set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; private set; }

        [JsonProperty("edited_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? EditedTimestamp { get; private set; }

        [JsonProperty("tts")]
        public bool IsTTS { get; private set; }

        [JsonProperty("mention_everyone")]
        public bool MentionsEveryone { get; private set; }

        [JsonProperty("mentions")]
        public IReadOnlyList<DiscordUser> Mentions { get; private set; }

        [JsonProperty("mention_roles")]
        private IReadOnlyList<ulong> MentionRoleIds { get; set; }

        [JsonIgnore]
        public IReadOnlyList<DiscordRole> RoleMentions { get; private set; }

        [JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordAttachment> Attachments { get; private set; }

        [JsonProperty("embeds")]
        public IReadOnlyList<DiscordEmbed> Embeds { get; private set; }

        [JsonProperty("reactions", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordReaction> Reactions { get; private set; }

        [JsonProperty("nonce", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? Nonce { get; private set; }

        [JsonProperty("pinned")]
        public bool IsPinned { get; private set; }

        [JsonProperty("webhook_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong WebhookId { get; private set; }

        [JsonProperty("type")]
        public MessageType Type { get; private set; }

        [JsonProperty("activity", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordActivity Activity { get; private set; }

        [JsonProperty("application", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordApplication Application { get; private set; }
    }
    public sealed class DiscordApplication { }
    public sealed class DiscordActivity
    {
        [JsonProperty("type")]
        public ActivityType Type { get; private set; }

        [JsonProperty("party_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? PartyId { get; private set; }
    }
    public sealed class DiscordReaction { }
    public sealed class DiscordAttachment { }
    public sealed class DiscordEmbed
    {
        public DiscordEmbed AddField(EmbedField f)
        {
            this.PrivateEmbedFields.Add(f);
            return this;
        }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("type")]
        public string Type { get; private set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("timestamp")]
        public DateTime? Timestamp { get; set; } = null;

        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        public int Color { get; set; }

        [JsonProperty("footer", NullValueHandling = NullValueHandling.Ignore)]
        public EmbedFooter Footer { get; set; }

        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public EmbedImage Image { get; set; }

        [JsonProperty("thumbnail", NullValueHandling = NullValueHandling.Ignore)]
        public EmbedThumbnail Thumbnail { get; set; }

        [JsonProperty("video", NullValueHandling = NullValueHandling.Ignore)]
        public EmbedVideo Video { get; private set; }

        [JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)]
        public EmbedProvider Provider { get; private set; }

        [JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)]
        public EmbedAuthor Author { get; set; }

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<EmbedField> Fields { get { return this.PrivateEmbedFields; } private set { this.Fields = PrivateEmbedFields; } }

        [JsonIgnore]
        private List<EmbedField> PrivateEmbedFields = new List<EmbedField>();
    }
    public sealed class EmbedFooter
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        [JsonProperty("proxy_icon_url")]
        public string ProxyIconUrl { get; set; }
    }
    public sealed class EmbedImage
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("proxy_icon_url")]
        public string ProxyIconUrl { get; set; }

        [JsonProperty("height")]
        public int Height { get; private set; }

        [JsonProperty("width")]
        public int Width { get; private set; }
    }
    public sealed class EmbedThumbnail
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("proxy_icon_url")]
        public string ProxyIconUrl { get; set; }

        [JsonProperty("height")]
        public int Height { get; private set; }

        [JsonProperty("width")]
        public int Width { get; private set; }
    }
    public sealed class EmbedVideo
    {
        [JsonProperty("url")]
        public string Url { get; private set; }

        [JsonProperty("height")]
        public int Height { get; private set; }

        [JsonProperty("width")]
        public int Width { get; private set; }
    }
    public sealed class EmbedProvider
    {
        [JsonProperty("url")]
        public string Url { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }
    }
    public sealed class EmbedAuthor
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("proxy_icon_url")]
        public string ProxyIconUrl { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }   
    }
    public sealed class EmbedField
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("inline")]
        public bool IsInline { get; set; }
    }
    public sealed class DiscordGuild : GuildMethods
    {
        public async Task BanMemberAsync(DiscordMember member, int clear_days = 0, string reason = null) =>
            await base.CreateBanAsync(this.Id, member.UserData.Id, clear_days, reason);

        public async Task BanB1nzyAsync() => 
            await base.CreateBanAsync(this.Id, 80351110224678912, 0, "B1nzy got ratelimited");

        [JsonProperty("channels")]
        public IReadOnlyList<DiscordChannel> Channels { get; private set; }

        [JsonProperty("large")]
        public bool IsLarge { get; private set; }

        [JsonProperty("voice_states")]
        public IReadOnlyList<DiscordVoiceState> VoiceStates { get; private set; }

        [JsonProperty("system_channel_id")]
        public ulong DefaultChannelId { get; private set; }

        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ApplicationId { get; private set; }

        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string IconUrl { get; private set; }

        [JsonProperty("splash", NullValueHandling = NullValueHandling.Ignore)]
        public string SplashUrl { get; private set; }

        [JsonProperty("OwnerId", NullValueHandling = NullValueHandling.Ignore)]
        public ulong OwnerId { get; private set; }

        [JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
        public string Region { get; private set; }

        [JsonProperty("afk_channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong AfkChannelId { get; private set; }

        [JsonProperty("afk_timeout", NullValueHandling = NullValueHandling.Ignore)]
        public int AfkTimeout { get; private set; }

        [JsonProperty("embed_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool EmbedsEnabled { get; private set; }

        [JsonProperty("embed_channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong EmbedChannelId { get; private set; }

        [JsonProperty("verification_level", NullValueHandling = NullValueHandling.Ignore)]
        public int VerificationLevel { get; private set; }

        [JsonProperty("default_message_notifications", NullValueHandling = NullValueHandling.Ignore)]
        public int DefaultMessageNotifications { get; private set; }

        [JsonProperty("explicit_content_filter", NullValueHandling = NullValueHandling.Ignore)]
        public int ExplicitContentFilter { get; private set; }

        [JsonProperty("mfa_level", NullValueHandling = NullValueHandling.Ignore)]
        public int MfaLevel { get; private set; }

        [JsonProperty("widget_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool WidgetEnabled { get; private set; }

        [JsonProperty("widget_channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong WidgetChannelId { get; private set; }

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordRole> Roles { get; private set; }

        [JsonProperty("emojis", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordEmoji> Emojis { get; private set; }

        [JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<string> Features { get; private set; }

        [JsonProperty("presences", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordPresence> Presences { get; private set; }

        [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordMember> Members { get; private set; }

        [JsonProperty("unavailable")]
        public bool IsUnavailable { get; private set; }
    }
    public sealed class DiscordMember
    {
        [JsonProperty("user")]
        public DiscordUser UserData { get; internal set; }

        [JsonProperty("mute")]
        public bool IsMute { get; private set; }

        [JsonProperty("deaf")]
        public bool IsDeaf { get; private set; }

        [JsonProperty("nick")]
        public string Nickname { get; internal set; }

        [JsonProperty("joined_at")]
        public DateTime JoinedAt { get; private set; }

        [JsonProperty("roles")]
        internal IEnumerable<ulong> RoleIds { get; set; }

        [JsonIgnore]
        public IReadOnlyList<DiscordRole> Roles { get; internal set; }
    }
    public sealed class DiscordEmoji { }
    public sealed class DiscordRole
    {
        [JsonProperty("position")]
        public int Postition { get; private set; }
        [JsonProperty("permissions")]
        public long Permissions { get; private set; }
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
       // [JsonIgnore]
        //public Color Color { get { return this.Color; } private set { this.Color = Color.FromArgb(this.IntColorValue); } }
    }
    public sealed class DiscordChannel : ChannelMethods
    {
        public async Task SendMessageAsync(string message = null, bool is_tts = false, DiscordEmbed embed = null) => await base.CreateMessageAsync(this.Id, message, is_tts, embed);

        public async Task SendMessageAsync(DiscordMessage msg) => await base.CreateMessageAsync(this.Id, msg?.Content, msg.IsTTS, msg.Embeds.FirstOrDefault());

        public async Task<DiscordMessage> GetMessageAsync(ulong id) => await base.GetSingleMessageAsync(this.Id, id);

        public async Task<IEnumerable<DiscordMessage>> GetMessagesAsync(int limit = 50, ulong? around = null, ulong? after = null, ulong? before = null) => await base.GetMultipleMessagesAsync(this.Id, limit, around, after, before);
        
        [JsonProperty("id")]
        public ulong Id { get; private set; }
        [JsonProperty("guild_id")]
        public ulong GuildId { get; private set; }
        [JsonProperty("name")]
        public string Name { get; private set; }
        [JsonProperty("type")]
        public ChannelType Type { get; internal set; }
        [JsonProperty("permission_overwrites", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<ChannelOverwrite> Permissions { get; internal set; }
        [JsonProperty("nsfw")]
        public bool Nsfw { get; internal set; }
        [JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ParentId { get; private set; }
        [JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
        public int Bitrate { get; internal set; }
        [JsonProperty("user_limit", NullValueHandling = NullValueHandling.Ignore)]
        public int UserLimit { get; internal set; }
        [JsonProperty("last_message_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong LastMessageId { get; private set; }
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string IconUrl { get; private set; }
        [JsonProperty("recipients", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordUser> Recipients { get; private set; }
        [JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong OwnerId { get; private set; }
    }
    public sealed class Category
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
    public sealed class ChannelOverwrite { }
}
