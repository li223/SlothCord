using SlothCord.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlothCord.SlothCord.Client
{
    public delegate void SocketOpened(object sender);
    public delegate void SocketClosed(object sender, string reason);
    public delegate void Heartbeated(object sender);
    public delegate void GuildsDownloaded(object sender, IEnumerable<DiscordGuild> Guilds);
}
