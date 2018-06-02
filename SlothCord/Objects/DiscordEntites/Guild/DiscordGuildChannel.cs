using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlothCord.Objects
{
    public sealed class DiscordGuildChannel : ChannelMethods
    {
        public async Task<DiscordChannel> ModifyAsync(string name = null, int? position = null, string topic = null, bool? nsfw = null, int? bitrate = null, int? user_limit = null, IEnumerable<GuildChannelOverwrite> permission_overwrites = null, ulong? parent_id = null)
            => await base.ModifyGuildChannelAsync(this.Id, name, position, topic, nsfw, bitrate, user_limit, permission_overwrites, parent_id);

        public async Task<DiscordMessage> SendFileAsync(string file_path, string message = null)
            => await base.CreateMessageWithFile(this.Id, file_path, message).ConfigureAwait(false);

        public async Task<DiscordMessage> PingB1nzyAsync()
            => await base.CreateMessageAsync(this.Id, "<&!80351110224678912>", false, null).ConfigureAwait(false);

        public async Task<DiscordGuildInvite?> DeleteInviteAsync(string code)
            => await base.DeleteDiscordInviteAsync(code).ConfigureAwait(false);

        public async Task<IEnumerable<DiscordGuildInvite>> GetInvitesAsync()
            => await base.GetChannelInvitesAsync(this.Id).ConfigureAwait(false);

        public async Task DeleteMessageAsync(ulong message_id)
            => await base.DeleteChannelMessageAsync(this.Id, message_id).ConfigureAwait(false);

        public async Task DeleteMessageAsync(DiscordMessage message)
            => await base.DeleteChannelMessageAsync(this.Id, message.Id).ConfigureAwait(false);

        public async Task BulkDeleteAsync(IEnumerable<ulong> ids)
            => await base.BulkDeleteGuildMessagesAsync(this.GuildId, this.Id, ids).ConfigureAwait(false);

        public async Task BulkDeleteAsync(IEnumerable<DiscordMessage> msgs)
            => await base.BulkDeleteGuildMessagesAsync(this.GuildId, this.Id, msgs.Select(x => x.Id) as IEnumerable<ulong>).ConfigureAwait(false);

        public async Task<DiscordMessage> SendMessageAsync(string message = null, bool is_tts = false, DiscordEmbed embed = null)
            => await base.CreateMessageAsync(this.Id, message, is_tts, embed).ConfigureAwait(false);

        public async Task<DiscordMessage> SendMessageAsync(DiscordMessage msg)
            => await base.CreateMessageAsync(this.Id, msg?.Content, msg.IsTTS, msg.Embeds.FirstOrDefault()).ConfigureAwait(false);

        public async Task<DiscordMessage> GetMessageAsync(ulong id)
            => await base.GetSingleMessageAsync(this.Id, id).ConfigureAwait(false);

        public async Task<IEnumerable<DiscordMessage>> GetMessagesAsync(int limit = 50, ulong? around = null, ulong? after = null, ulong? before = null)
            => await base.GetMultipleMessagesAsync(this.Id, limit, around, after, before).ConfigureAwait(false);

        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("guild_id")]
        public ulong? GuildId { get; private set; }

        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("type")]
        public ChannelType Type { get; private set; }

        [JsonProperty("permission_overwrites")]
        public IEnumerable<GuildChannelOverwrite> Permissions { get; internal set; }

        [JsonProperty("nsfw")]
        public bool Nsfw { get; internal set; }

        [JsonProperty("parent_id")]
        public ulong? ParentId { get; private set; }

        [JsonProperty("bitrate")]
        public int? Bitrate { get; internal set; }

        [JsonProperty("user_limit")]
        public int? UserLimit { get; internal set; }

        [JsonProperty("last_message_id")]
        public ulong? LastMessageId { get; private set; }

        [JsonProperty("recipients")]
        public IEnumerable<DiscordUser> Recipients { get; private set; }

        [JsonIgnore]
        public string Mention { get => $"<@#{this.Id}>"; }
    }
}