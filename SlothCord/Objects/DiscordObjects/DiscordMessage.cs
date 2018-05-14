using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlothCord.Objects
{

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
        public DiscordActivity Activity { get; private set; }

        [JsonProperty("application")]
        public DiscordApplication Application { get; private set; }
    }
}
