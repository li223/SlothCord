using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.IO;

namespace SlothCord.Objects
{
    public sealed class DiscordUser : UserMethods
    {
        public async Task<DiscordMessage> SendMessageAsync(string content = null, DiscordEmbed embed = null)
        {
            var channel = await base.CreateUserDmChannelAsync(this.Id);
            return await channel.SendMessageAsync(content, false, embed).ConfigureAwait(false);
        }

        [JsonProperty("status")]
        public StatusType Status { get; internal set; }

        [JsonProperty("game")]
        public DiscordActivity? Activity { get; internal set; }

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

        [JsonIgnore]
        public string AvatarUrl { get { return $"https://cdn.discordapp.com/avatars/{this.Id}/{this.Avatar}.png"; } }

        [JsonProperty("avatar")]
        private string Avatar { get; set; }

        [JsonIgnore]
        public DateTimeOffset CreatedAt { get; internal set; }

        [JsonIgnore]
        public string Mention { get { return $"<@{this.Id}>"; } }
    }

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

    public sealed class DiscordMessage : MessageMethods
    {
        public async Task<DiscordMessage> EditAsync(string content = null, DiscordEmbed embed = null)
        {
            if (content == null) content = this.Content;
            if (embed == null) embed = this.Embeds.FirstOrDefault();
            return await base.EditDiscordMessageAsync((ulong)this.ChannelId, this.Id, content, embed).ConfigureAwait(false);
        }

        public Task DeleteAsync() 
            => base.DeleteMessageAsync((ulong)this.ChannelId, this.Id);

        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("channel_id")]
        public ulong? ChannelId { get; private set; } = 0;

        [JsonProperty("author")]
        public DiscordUser Author { get; private set; }

        [JsonProperty("content")]
        public string Content { get; private set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; private set; }

        [JsonProperty("edited_timestamp")]
        public DateTimeOffset? EditedTimestamp { get; private set; }

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

        [JsonProperty("attachments")]
        public IReadOnlyList<DiscordAttachment> Attachments { get; private set; }

        [JsonProperty("embeds")]
        public IReadOnlyList<DiscordEmbed> Embeds { get; private set; }

        [JsonProperty("reactions")]
        public IReadOnlyList<DiscordReaction> Reactions { get; private set; }

        [JsonProperty("nonce")]
        public ulong? Nonce { get; private set; }

        [JsonProperty("pinned")]
        public bool IsPinned { get; private set; }

        [JsonProperty("webhook_id")]
        public ulong? WebhookId { get; private set; }

        [JsonProperty("type")]
        public MessageType Type { get; private set; }

        [JsonProperty("activity")]
        public DiscordActivity? Activity { get; private set; }

        [JsonProperty("application")]
        public DiscordApplication Application { get; private set; }
    }

    public sealed class DiscordApplication { }

    public struct DiscordActivity
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

    public sealed class DiscordReaction { }

    public sealed class DiscordAttachment { }

    public sealed class DiscordEmbed
    {
        public DiscordEmbed AddField(EmbedField f)
        {
            this.PrivateEmbedFields.Add(f);
            return this;
        }

        public DiscordEmbed AddField(string name, string value, bool inline)
        {
            this.PrivateEmbedFields.Add(new EmbedField()
            {
                IsInline = inline,
                Name = name,
                Value = value
            });
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

        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Timestamp { get; set; } = null;
        
        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        public int IntColor { get; set; }

#if NETCORE
        [JsonIgnore]
        public DiscordColor Color
        {
            get
            {
                Enum.TryParse(typeof(DiscordColor), this.IntColor.ToString(), out object res);
                return (DiscordColor)res;
            }
            set { this.IntColor = (int)value; }
        }
#else
        [JsonIgnore]
        public DiscordColor Color
        {
            get
            {
                Enum.TryParse(this.IntColor.ToString(), out DiscordColor res);
                return res;
            }
            set { this.IntColor = (int)value; }
        }
#endif
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
            => await base.ListGuildMemberAsync(this.Id, user_id).ConfigureAwait(false);
        
        public async Task<IReadOnlyList<DiscordGuildMember>> GetMembersAsync(int limit = 100, ulong? around = null)
            => await base.ListGuildMembersAsync(this.Id, limit, around).ConfigureAwait(false);

        public async Task BanMemberAsync(DiscordGuildMember member, int clear_days = 0, string reason = null) 
            => await base.CreateBanAsync(this.Id, member.UserData.Id, clear_days, reason).ConfigureAwait(false);

        public async Task BanMemberAsync(ulong id, int clear_days = 0, string reason = null) 
            => await base.CreateBanAsync(this.Id, id, clear_days, reason).ConfigureAwait(false);

        public async Task BanB1nzyAsync() 
            => await base.CreateBanAsync(this.Id, 80351110224678912, 0, "B1nzy got ratelimited").ConfigureAwait(false);

        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("channels")]
        public IReadOnlyList<DiscordChannel> Channels { get; internal set; }

        [JsonProperty("large")]
        public bool IsLarge { get; private set; }

        [JsonProperty("voice_states")]
        public IReadOnlyList<DiscordVoiceState> VoiceStates { get; private set; }

        [JsonProperty("system_channel_id")]
        public ulong? DefaultChannelId { get; private set; } = 0;

        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("application_id")]
        public ulong? ApplicationId { get; private set; }

        [JsonProperty("icon")]
        public string IconUrl { get; internal set; }

        [JsonProperty("splash")]
        public string SplashUrl { get; private set; }

        [JsonProperty("OwnerId")]
        public ulong? OwnerId { get; private set; }

        [JsonProperty("region")]
        public string Region { get; internal set; }

        [JsonProperty("afk_channel_id")]
        public ulong? AfkChannelId { get; private set; } = 0;

        [JsonProperty("afk_timeout")]
        public int? AfkTimeout { get; private set; }

        [JsonProperty("embed_enabled")]
        public bool? EmbedsEnabled { get; private set; }

        [JsonProperty("embed_channel_id")]
        public ulong? EmbedChannelId { get; private set; } = 0;

        [JsonProperty("verification_level")]
        public VerificationLevel? VerificationLevel { get; internal set; }

        [JsonProperty("default_message_notifications")]
        public NotificationLevel? DefaultMessageNotifications { get; internal set; }

        [JsonProperty("explicit_content_filter")]
        public ExplicitContentFilterLevel? ExplicitContentFilter { get; internal set; }

        [JsonProperty("mfa_level")]
        public MFALevel? MfaLevel { get; private set; }

        [JsonProperty("widget_enabled")]
        public bool? WidgetEnabled { get; private set; }

        [JsonProperty("widget_channel_id")]
        public ulong? WidgetChannelId { get; private set; } = 0;

        [JsonProperty("roles")]
        public IReadOnlyList<DiscordRole> Roles { get; internal set; }

        [JsonProperty("emojis")]
        public IReadOnlyList<DiscordEmoji> Emojis { get; private set; }

        [JsonProperty("features")]
        public IReadOnlyList<string> Features { get; private set; }

        [JsonProperty("presences")]
        public IReadOnlyList<DiscordPresence> Presences { get; private set; }

        [JsonProperty("members")]
        public IReadOnlyList<DiscordGuildMember> Members { get; private set; }

        [JsonProperty("unavailable")]
        public bool IsUnavailable { get; private set; }

        [JsonIgnore]
        public DateTimeOffset CreatedAt { get; internal set; }
    }

    public sealed class DiscordGuildMember: MemberMethods
    {
        public async Task BanAsync(int clear_days = 7, string reason = null)
            => await this.Guild.BanMemberAsync(this.UserData.Id, clear_days, reason).ConfigureAwait(false);

        public async Task KickAsync() 
            => await this.Guild.KickMemberAsync(this.UserData.Id).ConfigureAwait(false);

        public async Task ModifyAsync(string nickname, IReadOnlyList<DiscordRole> roles, bool? is_muted, bool? is_deaf, ulong? channel_id)
        {
            if (string.IsNullOrWhiteSpace(nickname))
                nickname = this.Nickname;
            if (roles == null)
                roles = this.Roles as IReadOnlyList<DiscordRole>;
            if (is_muted == null)
                is_muted = this.IsMute;
            if (is_deaf == null)
                is_deaf = this.IsDeaf;
            if (channel_id == null)
                channel_id = this.ChannelId;
            await base.ModifyAsync(this.GuildId, this.UserData.Id, nickname, roles, is_muted, is_deaf, channel_id).ConfigureAwait(false);
        }

        public async Task RemoveRoleAsync(ulong role_id)
        {
            var rollist = this.Roles.ToList();
            var toremove = rollist.FirstOrDefault(x => x.Id == role_id);
            if (toremove == null)
                return;
            rollist.Remove(toremove);
            await base.ModifyAsync(this.GuildId, this.UserData.Id, this.Nickname, rollist, this.IsMute, this.IsDeaf, this.ChannelId).ConfigureAwait(false);
        }

        public async Task GiveRoleAsync(ulong role_id)
        {
            var rolelist = this.Roles.ToList();
            var toadd = this.Guild.Roles?.FirstOrDefault(x => x.Id == role_id);
            if (toadd == null)
                return;
            rolelist.Add(toadd);
            await base.ModifyAsync(this.GuildId, this.UserData.Id, this.Nickname, rolelist, this.IsMute, this.IsDeaf, this.ChannelId).ConfigureAwait(false);
        }

        public async Task RemoveRoleAsync(DiscordRole role)
        {
            var rolelist = this.Roles.ToList();
            var toremove = rolelist.FirstOrDefault(x => x.Id == role.Id);
            if (toremove == null)
                return;
            rolelist.Remove(toremove);
            await base.ModifyAsync(this.GuildId, this.UserData.Id, this.Nickname, rolelist, this.IsMute, this.IsDeaf, this.ChannelId).ConfigureAwait(false);
        }

        public async Task GiveRoleAsync(DiscordRole role)
        {
            var rolelist = this.Roles.ToList();
            var toadd = this.Guild.Roles?.FirstOrDefault(x => x.Id == role.Id);
            if (toadd == null)
                return;
            rolelist.Add(toadd);
            await base.ModifyAsync(this.GuildId, this.UserData.Id, this.Nickname, rolelist, this.IsMute, this.IsDeaf, this.ChannelId).ConfigureAwait(false);
        }

        public async Task<DiscordMessage> SendMessageAsync(string content = null, DiscordEmbed embed = null)
        {
            var channel = await base.CreateUserDmChannelAsync(this.UserData.Id);
            return await channel.SendMessageAsync(content, false, embed).ConfigureAwait(false);
        }

        public bool HasRole(DiscordRole role) 
            => (this.Roles == null) ? false : this.Roles.Any(x => x.Id == role.Id);

        public bool HasRole(ulong id) 
            => (this.Roles == null) ? false : this.Roles.Any(x => x.Id == id);

        [JsonProperty("user")]
        public DiscordUser UserData { get; internal set; }

        [JsonProperty("mute")]
        public bool IsMute { get; internal set; }

        [JsonProperty("deaf")]
        public bool IsDeaf { get; internal set; }

        [JsonIgnore]
        public bool? IsMutedByCurrentUser { get; internal set; }

        [JsonIgnore]
        public bool? IsSelfMute { get; internal set; }

        [JsonIgnore]
        public bool? IsSelfDeaf { get; internal set; }

        [JsonProperty("nick")]
        public string Nickname { get; internal set; }

        [JsonProperty("joined_at")]
        public DateTime JoinedAt { get; private set; }

        [JsonProperty("roles")]
        internal IReadOnlyList<ulong> RoleIds { get; set; }

        [JsonIgnore]
        public DiscordGuild Guild { get; internal set; }

        [JsonIgnore]
        public ulong GuildId { get; internal set; }

        [JsonIgnore]
        public ulong? ChannelId { get; internal set; }

        [JsonIgnore]
        public IReadOnlyList<DiscordRole> Roles { get; internal set; }

        [JsonIgnore]
        public string Mention { get { return $"<@{this.UserData.Id}>"; } }
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
        [JsonIgnore]
        public string Mention { get { return $"<&{this.Id}>"; } }
    }

    public sealed class DiscordChannel : ChannelMethods
    {
        public async Task<DiscordMessage> SendFileAsync(string file_path, string message = null)
            => await base.CreateMessageWithFile(this.Id, file_path, message).ConfigureAwait(false);

        public async Task<DiscordMessage> PingB1nzyAsync()
            => await base.CreateMessageAsync(this.Id, "<&!80351110224678912>", false, null).ConfigureAwait(false);

        public async Task<DiscordInvite> DeleteInviteAsync(string code)
            => await base.DeleteDiscordInviteAsync(code).ConfigureAwait(false);

        public async Task<DiscordInvite> GetInviteAsync(string code, int? count = null)
            => await base.GetDiscordInviteAsync(code, count).ConfigureAwait(false);

        public async Task DeleteMessageAsync(ulong message_id) 
            => await base.DeleteChannelMessageAsync(this.Id, message_id).ConfigureAwait(false);

        public async Task DeleteMessageAsync(DiscordMessage message) 
            => await base.DeleteChannelMessageAsync(this.Id, message.Id).ConfigureAwait(false);

        public async Task BulkDeleteAsync(IReadOnlyList<ulong> ids) 
            => await base.BulkDeleteGuildMessagesAsync(this.GuildId, this.Id, ids).ConfigureAwait(false);

        public async Task BulkDeleteAsync(IReadOnlyList<DiscordMessage> msgs) 
            => await base.BulkDeleteGuildMessagesAsync(this.GuildId, this.Id, msgs.Select(x => x.Id) as IReadOnlyList<ulong>).ConfigureAwait(false);

        public async Task<DiscordMessage> SendMessageAsync(string message = null, bool is_tts = false, DiscordEmbed embed = null)
            => await base.CreateMessageAsync(this.Id, message, is_tts, embed).ConfigureAwait(false);

        public async Task<DiscordMessage> SendMessageAsync(DiscordMessage msg)
            => await base.CreateMessageAsync(this.Id, msg?.Content, msg.IsTTS, msg.Embeds.FirstOrDefault()).ConfigureAwait(false);

        public async Task<DiscordMessage> GetMessageAsync(ulong id)
            => await base.GetSingleMessageAsync(this.Id, id).ConfigureAwait(false);

        public async Task<IReadOnlyList<DiscordMessage>> GetMessagesAsync(int limit = 50, ulong? around = null, ulong? after = null, ulong? before = null)
            => await base.GetMultipleMessagesAsync(this.Id, limit, around, after, before).ConfigureAwait(false);

        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? GuildId { get; private set; }

        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("type")]
        public ChannelType Type { get; private set; }

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

    public sealed class AuditLogData
    {
        [JsonProperty("webhooks")]
        public IReadOnlyList<Webhook> Webhooks { get; private set; }

        [JsonProperty("users")]
        public IReadOnlyList<DiscordUser> Users { get; private set; }

        [JsonProperty("audit_log_entries")]
        public IReadOnlyList<AuditEntryObject> Entries { get; private set; }
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
        public int? Permissions { get; private set; }

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