using SlothCord.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SlothCord
{
    public delegate Task SocketOpenedEvent();
    public delegate Task SocketClosedEvent(string reason);
    public delegate Task HeartbeatedEvent();
    public delegate Task ReadyEvent();
    public delegate Task GuildsDownloadedEvent(IEnumerable<DiscordGuild> Guilds);
    public delegate Task GuildCreatedEvent(DiscordGuild guild);
    public delegate Task UnkownEvent(string message, string payload);
    public delegate Task MessageCreatedEvent(DiscordMessage message);
    public delegate Task MemberAddedEvent(DiscordGuild guild, DiscordGuildMember member);
    public delegate Task MemberRemovedEvent(DiscordGuild guild, DiscordUser user);
    public delegate Task CommandErroredEvent(Exception exception);
    public delegate Task ResumedEvent();
}
