using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlothCord.Objects
{
    public sealed class DiscordUser : UserMethods
    {
        public async Task<DiscordChannel> CreateDmAsync()
            => await base.CreateUserDmChannelAsync(this.Id).ConfigureAwait(false);

        public async Task<DiscordMessage> SendMessageAsync(string content = null, DiscordEmbed embed = null)
        {
            var channel = await base.CreateUserDmChannelAsync(this.Id).ConfigureAwait(false);
            return await channel.SendMessageAsync(content, false, embed).ConfigureAwait(false);
        }

        [JsonIgnore]
        public string AvatarUrl { get => $"https://cdn.discordapp.com/avatars/{this.Id}/{this.Avatar}.png"; }

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

        [JsonIgnore]
        public string Mention { get => $"<@{this.Id}>"; }

        [JsonProperty("status")]
        public StatusType Status { get; internal set; }

        [JsonProperty("game")]
        public DiscordActivity Activity { get; internal set; }

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
        private string Avatar { get; set; }
    }
}