using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlothCord
{
    public enum StatusType
    {
        Online = 0,
        Offline = 1,
        Invisible = 2,
        Idle = 3,
        DND = 4
    }
    public enum PlayingType
    {
        Playing = 0,
        Watching = 1,
        Listening = 2
    }
    public enum MessageType
    {
        Default = 0, 
        RecipientAdd = 1,
        RecipientRemove = 2,
        Call = 3,
        ChannelNameChange = 4,
        ChannelIconChange = 5,
        ChannelPinMessage = 6,
        GuildMemberJoin = 7
    }
    public enum ActivityType
    {
        Join = 1,
        Spectate = 2,
        Listen = 3,
        JoinRequest = 4
    }
    public enum TokenType
    {
        Bot = 0,
        Bearer = 1,
        User = 2
    }
    public enum OPCode : int
    {
        Dispatch = 0,
        Heartbeat = 1,
        Identify = 2,
        StatusUpdate = 3,
        VoiceStateUpdate = 4,
        VoiceServerPing = 5,
        Resume = 6,
        Reconnect = 7,
        RequestGuildMembers = 8,
        InvalidSession = 9,
        Hello = 10,
        HeartbeatAck = 11,
        Unknown = 12
    }
    public enum ChannelType
    {
        GuildText = 0,
        DirectMessage = 1,
        GuildVoice = 2,
    }
    public enum EventType
    {
        Ready = 0,
        TypingStart = 1,
        Guild_Create = 2
    }
}
