using SlothCord.Objects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SlothCord
{
    public delegate Task SocketOpened(object sender);
    public delegate Task SocketClosed(object sender, string reason);
    public delegate Task Heartbeated(object sender);
    public delegate Task GuildsDownloaded(object sender, IEnumerable<DiscordGuild> Guilds);
    public delegate Task GuildCreated(object sender, DiscordGuild guild);
    public delegate Task UnkownOpCode(object sender, int code, string name);
    public delegate Task UnkownEvent(object sender, int code, string payload);
    public delegate Task MessageCreated(object sender, DiscordMessage message);
}
