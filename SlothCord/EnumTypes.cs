using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlothCord
{
    public enum VerificationLevel : int
    {
        NONE = 0,
        LOW = 1,
        MEDIUM = 2,
        HIGH = 3,
        VERY_HIGH = 4
    }

    public enum MFALevel : int
    {
        NONE = 0,
        ELEVATED = 1
    }

    public enum ExplicitContentFilterLevel : int
    {
        DISABLED = 0,
        MEMBERS_WITHOUT_ROLES = 1,
        ALL_MEMBERS = 2
    }

    public enum NotificationLevel : int
    {
        ALL_MESSAGES = 0,
        ONLY_MENTIONS = 1
    }

    public enum DispatchType
    {
        READY = 0,
        PRESENCE_UPDATE = 1,
        GUILD_CREATE = 2,
        MESSAGE_CREATE = 3,
        TYPING_START = 4,
        CHANNEL_CREATE = 5,
        CHANNEL_UPDATE = 6,
        CHANNEL_DELETE = 7,
        CHANNEL_PINS_UPDATE = 8,
        GUILD_UPDATE = 9,
        GUILD_DELETE = 10,
        GUILD_BAN_ADD = 11,
        GUILD_BAN_REMOVE = 12,
        GUILD_EMOJIS_UPDATE = 13,
        GUILD_INTERGRATIONS_UPDATE = 14,
        GUILD_MEMBER_UPDATE = 15,
        GUILD_MEMBER_ADD = 16,
        GUILD_MEMBER_REMOVE = 17,
        GUILD_MEMBERS_CHUNK = 18,
        GUILD_ROLE_CREATE = 19,
        GUILD_ROLE_DELETE = 20,
        GUILD_ROLE_UPDATE = 21,
        MESSAGE_UPDATE = 22,
        MESSAGE_DELETE = 23,
        MESSAGE_DELETE_BULK = 24,
        MESSAGE_REACTION_ADDED = 25,
        MESSAGE_REACTION_REMOVE = 26,
        MESSAGE_REACTION_REMOVE_ALL = 27,
        USER_UPDATE = 28,
        VOICE_STATE_UPDATE = 29,
        VOICE_SERVER_UPDATE = 30,
        WEBHOOKS_UPDATE = 31
    }

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
