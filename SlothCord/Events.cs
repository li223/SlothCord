using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlothCord
{
    public delegate void OnGenericEvent(object sender, object e);
    public delegate void OnHttpError(object sender, string e);
    public delegate void OnMessageUpdate(object sender, DiscordMessage b, DiscordMessage a);
    public delegate void OnChannelEvent(object sender, DiscordChannel e);
    public delegate void OnTypingStart(object sender, TypingStartArgs e);
    public delegate void OnCommandError(object sender, string e);
    public delegate void OnGuildsDownloaded(object sender, IEnumerable<DiscordGuild> e);
    public delegate void OnMessageCreate(object sender, DiscordMessage e);
    public delegate void OnGuildAvailable(object sender, DiscordGuild e);
    public delegate void OnWebSocketClose(object sender, EventArgs e);
    public delegate void OnUnknownEvent(object sender, UnkownEventArgs e);
    public delegate void OnHeartbeat(object sender, string e);
    public delegate void OnReady(object sender, OnReadyArgs e);
    public delegate void OnClientError(object sender, ClientErroredArgs e);
    public delegate void OnSocketDataReceived(object sender, string e);
    public delegate void OnPresenceUpdate(object sender, PresenceUpdateArgs e);

    public class TypingStartArgs : EventArgs
    {
        public DiscordChannel Channel;
        public ulong ChannelId;
        public DiscordGuildMember Member = null;
        public ulong UserId;
        public DiscordGuild Guild = null;
    }
    public class PresenceUpdateArgs : EventArgs
    {
        public DiscordGuildMember MemberBefore { get; internal set; }
        public DiscordGuildMember MemberAfter { get; internal set; }
    }
    public sealed class UnkownEventArgs : EventArgs
    {
        public string EventName { get; internal set; }
        public int OPCode { get; internal set; }
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
        public Exception Exception { get; internal set; }  
    }
}
