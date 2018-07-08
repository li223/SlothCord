using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlothCord.Objects
{
    public sealed class DiscordGuildMember : MemberMethods
    {
        public async Task BanAsync(int clear_days = 7, string reason = null)
            => await this.Guild.BanMemberAsync(this.UserData.Id, clear_days, reason).ConfigureAwait(false);

        public async Task KickAsync()
            => await this.Guild.KickMemberAsync(this.UserData.Id).ConfigureAwait(false);

        public async Task ModifyAsync(string nickname, IEnumerable<DiscordGuildRole> roles, bool? is_muted, bool? is_deaf, ulong? channel_id)
        {
            if (string.IsNullOrWhiteSpace(nickname)) nickname = this.Nickname;
            if (roles == null) roles = this.Roles as IEnumerable<DiscordGuildRole>;
            if (is_muted == null) is_muted = this.IsMute;
            if (is_deaf == null) is_deaf = this.IsDeaf;
            if (channel_id == null) channel_id = this.ChannelId;
            await base.ModifyAsync(this.Guild.Id, this.UserData.Id, nickname, roles, is_muted, is_deaf, channel_id).ConfigureAwait(false);
        }

        public async Task RemoveRoleAsync(ulong role_id)
            => await base.DeleteRoleAsync(this.GuildId, this.UserData.Id, role_id).ConfigureAwait(false);

        public async Task AddRoleAsync(ulong role_id)
            => await base.PutRoleAsync(this.GuildId, this.UserData.Id, role_id).ConfigureAwait(false);

        public async Task RemoveRoleAsync(DiscordGuildRole? role)
            => await base.DeleteRoleAsync(this.GuildId, this.UserData.Id, role.Value.Id).ConfigureAwait(false);

        public async Task AddRoleAsync(DiscordGuildRole? role)
             => await base.PutRoleAsync(this.GuildId, this.UserData.Id, role.Value.Id).ConfigureAwait(false);

        public async Task<DiscordMessage> SendMessageAsync(string content = null, DiscordEmbed embed = null)
        {
            var channel = await base.CreateUserDmChannelAsync(this.UserData.Id).ConfigureAwait(false);
            return await channel.SendMessageAsync(content, false, embed).ConfigureAwait(false);
        }

        public bool HasRole(DiscordGuildRole role)
            => (this.Roles == null) ? false : this.Roles.Any(x => x.Value.Id == role.Id);

        public bool HasRole(ulong id)
            => (this.Roles == null) ? false : this.Roles.Any(x => x.Value.Id == id);

        [JsonProperty("user")]
        public DiscordUser UserData { get; internal set; }

        [JsonProperty("mute")]
        public bool IsMute { get; private set; }

        [JsonProperty("deaf")]
        public bool IsDeaf { get; private set; }

        [JsonProperty("nick")]
        public string Nickname { get; private set; }

        [JsonProperty("joined_at")]
        public DateTime JoinedAt { get; private set; }

        [JsonProperty("roles")]
        internal IEnumerable<ulong> RoleIds { get; set; }

        [JsonProperty("guild_id")]
        public ulong GuildId { get; private set; }

        [JsonIgnore]
        public IReadOnlyList<DiscordGuildRole?> Roles { get; internal set; }

        [JsonIgnore]
        public DiscordGuild Guild { get; internal set; }

        [JsonIgnore]
        public ulong? ChannelId { get; internal set; }

        [JsonIgnore]
        public string Mention { get => $"<@!{this.UserData.Id}>"; }

    }
}
