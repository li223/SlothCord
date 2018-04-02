using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlothCord
{
    public delegate void OnGuildsDownloaded(object sender, IEnumerable<DiscordGuild> e);
    public delegate void OnMessageCreate(object sender, DiscordMessage e);
    public delegate void OnGuildAvailable(object sender, DiscordGuild e);
    public delegate void OnWebSocketClose(object sender, EventArgs e);
    public delegate void OnUnknownEvent(object sender, UnkownEventArgs e);
    public delegate void OnHeartbeat(object sender, OnHeartbeatArgs e);
    public delegate void OnReady(object sender, OnReadyArgs e);
    public delegate void OnClientError(object sender, ClientErroredArgs e);
    public delegate void OnSocketDataReceived(object sender, SocketDataArgs e);
    public delegate void OnPresenceUpdate(object sender, PresenceUpdateArgs e);
    public class PresenceUpdateArgs : EventArgs
    {
        public DiscordMember MemberBefore { get; internal set; }
        public DiscordMember MemberAfter { get; internal set; }
    }
    public sealed class UnkownEventArgs : EventArgs
    {
        public string EventName { get; internal set; }
        public int OPCode { get; internal set; }
    }
    public sealed class OnHeartbeatArgs : EventArgs
    {
        public string Message { get; internal set; }
    }
    public sealed class OnReadyArgs : EventArgs
    {
        public int GatewayVersion { get; internal set; }
        public string SessionId { get; internal set; }

    }
    public sealed class ClientErroredArgs : EventArgs
    {
        public string Message { get; internal set; }
        public string Source { get; internal set; }
    }
    public sealed class SocketDataArgs : EventArgs
    {
        public string Data { get; internal set; }
    }
}
