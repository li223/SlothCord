using SlothCord.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SlothCord
{
    public delegate Task SocketOpened(object sender);
    public delegate Task SocketClosed(object sender, string reason);
    public delegate Task Heartbeated(object sender);
    public delegate Task GuildsDownloaded(object sender, IEnumerable<DiscordGuild> Guilds);
    public delegate Task GuildCreated(object sender, DiscordGuild guild);
}
