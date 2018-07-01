using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        {
            var rollist = this.Roles.ToList();
            var toremove = rollist?.FirstOrDefault(x => x.Id == role_id);
            if (toremove == null) return;
            rollist.Remove((DiscordGuildRole)toremove);
            await base.ModifyAsync(this.Guild.Id, this.UserData.Id, this.Nickname, rollist, this.IsMute, this.IsDeaf, this.ChannelId).ConfigureAwait(false);
        }

        public async Task GiveRoleAsync(ulong role_id)
        {
            var rolelist = this.Roles.ToList();
            var toadd = this.Guild.Roles?.FirstOrDefault(x => x.Id == role_id);
            if (toadd == null) return;
            rolelist.Add((DiscordGuildRole)toadd);
            await base.ModifyAsync(this.Guild.Id, this.UserData.Id, this.Nickname, rolelist, this.IsMute, this.IsDeaf, this.ChannelId).ConfigureAwait(false);
        }

        public async Task RemoveRoleAsync(DiscordGuildRole role)
        {
            var rolelist = this.Roles.ToList();
            var toremove = rolelist?.FirstOrDefault(x => x.Id == role.Id);
            if (toremove == null) return;
            rolelist.Remove((DiscordGuildRole)toremove);
            await base.ModifyAsync(this.Guild.Id, this.UserData.Id, this.Nickname, rolelist, this.IsMute, this.IsDeaf, this.ChannelId).ConfigureAwait(false);
        }

        public async Task GiveRoleAsync(DiscordGuildRole role)
        {
            var rolelist = this.Roles.ToList();
            var toadd = this.Guild.Roles?.FirstOrDefault(x => x.Id == role.Id);
            if (toadd == null) return;
            rolelist.Add((DiscordGuildRole)toadd);
            await base.ModifyAsync(this.Guild.Id, this.UserData.Id, this.Nickname, rolelist, this.IsMute, this.IsDeaf, this.ChannelId).ConfigureAwait(false);
        }

        public async Task<DiscordMessage> SendMessageAsync(string content = null, DiscordEmbed embed = null)
        {
            var channel = await base.CreateUserDmChannelAsync(this.UserData.Id).ConfigureAwait(false);
            return await channel.SendMessageAsync(content, false, embed).ConfigureAwait(false);
        }

        public bool HasRole(DiscordGuildRole role)
            => (this.Roles == null) ? false : this.Roles.Any(x => x.Id == role.Id);

        public bool HasRole(ulong id)
            => (this.Roles == null) ? false : this.Roles.Any(x => x.Id == id);

        [JsonProperty("user")]
        public DiscordUser UserData { get; private set; }

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

        [JsonIgnore]
        public IEnumerable<DiscordGuildRole> Roles { get; internal set; }

        [JsonIgnore]
        public DiscordGuild Guild { get; internal set; }

        [JsonIgnore]
        public ulong? ChannelId { get; internal set; }

        [JsonIgnore]
        public string Mention { get => $"<@!{this.UserData.Id}>"; }

    }
}
