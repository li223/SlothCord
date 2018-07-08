using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlothCord.Objects
{
    public sealed class DiscordChannel : ChannelMethods
    {
        public async Task<DiscordMessage> SendFileAsync(string file_path, string message = null)
            => await base.CreateMessageWithFile(this.Id, file_path, message).ConfigureAwait(false);

        public async Task DeleteMessageAsync(ulong message_id)
            => await base.DeleteChannelMessageAsync(this.Id, message_id).ConfigureAwait(false);

        public async Task DeleteMessageAsync(DiscordMessage message)
            => await base.DeleteChannelMessageAsync(this.Id, message.Id).ConfigureAwait(false);

        public async Task BulkDeleteAsync(IEnumerable<ulong> ids)
            => await base.BulkDeleteMessagesAsync(this.Id, ids.ToArray()).ConfigureAwait(false);

        public async Task BulkDeleteAsync(IEnumerable<DiscordMessage> msgs)
            => await base.BulkDeleteMessagesAsync(this.Id, (msgs.Select(x => x.Id).ToArray())).ConfigureAwait(false);
        
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

        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("type")]
        public ChannelType Type { get; private set; }

        [JsonProperty("last_message_id")]
        public ulong LastMessageId { get; private set; }

        [JsonProperty("icon")]
        public string Icon { get; private set; }

        [JsonProperty("recipients")]
        public IEnumerable<DiscordUser> Recipients { get; private set; }

        [JsonProperty("owner_id")]
        public ulong OwnerId { get; private set; }

        [JsonIgnore]
        public string IconUrl { get => $"https://discordapp.com/channels/{this.Id}/{this.Icon}.png"; }

        [JsonIgnore]
        public DiscordUser Owner { get; internal set; }

        [JsonIgnore]
        public string Mention { get => $"<@#{this.Id}>"; }
    }
}