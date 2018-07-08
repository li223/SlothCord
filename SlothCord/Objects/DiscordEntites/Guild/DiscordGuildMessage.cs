using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace SlothCord.Objects
{
    public sealed class DiscordGuildMessage : DiscordMessage
    {
        public DiscordGuildMessage(DiscordGuild guild)
        {
            this.MemberMentions = guild.Members.Where(x => MentionRoleIds.Any(a => a == x.UserData.Id)) as IEnumerable<DiscordGuildMember>;
        }

        [JsonProperty("webhook_id")]
        public ulong? WebhookId { get; private set; }

        [JsonProperty("mention_roles")]
        public IEnumerable<ulong> MentionRoleIds { get; private set; }

        [JsonIgnore]
        public IEnumerable<DiscordGuildMember> MemberMentions { get; private set; }

        [JsonIgnore]
        public IEnumerable<DiscordGuildRole> RoleMentions { get; private set; }

        [JsonIgnore]
        public DiscordGuildMember MemberAuthor { get; internal set; }
    }
}
