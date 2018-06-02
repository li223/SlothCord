using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlothCord.Objects
{
    public class DiscordMessage : MessageMethods
    {
        public async Task<DiscordMessage> EditAsync(string content = null, DiscordEmbed embed = null)
        {
            if (content == null) content = this.Content;
            if (embed == null) embed = this.Embeds.FirstOrDefault();
            return await base.EditDiscordMessageAsync((ulong)this.ChannelId, this.Id, content, embed).ConfigureAwait(false);
        }

        public async Task DeleteAsync()
            => await base.DeleteMessageAsync((ulong)this.ChannelId, this.Id).ConfigureAwait(false);
        
        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("channel_id")]
        public ulong? ChannelId { get; private set; }

        [JsonProperty("author")]
        public DiscordUser UserAuthor { get; private set; }

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
        public IReadOnlyList<DiscordUser> UserMentions { get; private set; }

        [JsonProperty("attachments")]
        public IReadOnlyList<DiscordAttachment> Attachments { get; private set; }

        [JsonProperty("embeds")]
        public IReadOnlyList<DiscordEmbed> Embeds { get; private set; }

        [JsonProperty("reactions")]
        public IReadOnlyList<DiscordGuildEmoji> Reactions { get; private set; }

        [JsonProperty("nonce")]
        public ulong? Nonce { get; private set; }

        [JsonProperty("pinned")]
        public bool IsPinned { get; private set; }

        [JsonProperty("type")]
        public MessageType Type { get; private set; }

        [JsonProperty("activity")]
        public DiscordMessageActivity Activity { get; private set; }

        [JsonProperty("application")]
        public DiscordMessageApplication Application { get; private set; }
    }
}