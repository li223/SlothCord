using System;

namespace SlothCord.Objects
{
    public enum DiscordColor : int
    {
        Blue = 0x0021ff,
        Yellow = 0xe3ed31,
        Orange = 0xE86800,
        White = 0xFFFFFF,
        Purple = 0x952097,
        Lilac = 0xD58BFF,
        Gray = 0x847671,
        Red = 0xE32B15,
        Green = 0x1ca50f,
        DarkButNotBlack = 0x2C2F33
    }

    public enum AuditActionType
    {
        GUILD_UPDATE = 1,
        CHANNEL_CREATE = 10,
        CHANNEL_UPDATE = 11,
        CHANNEL_DELETE = 12,
        CHANNEL_OVERWRITE_CREATE = 13,
        CHANNEL_OVERWRITE_UPDATE = 14,
        CHANNEL_OVERWRITE_DELETE = 15,
        MEMBER_KICK = 20,
        MEMBER_PRUNE = 21,
        MEMBER_BAN_ADD = 22,
        MEMBER_BAN_REMOVE = 23,
        MEMBER_UPDATE = 24,
        MEMBER_ROLE_UPDATE = 25,
        ROLE_CREATE = 30,
        ROLE_UPDATE = 31,
        ROLE_DELETE = 32,
        INVITE_CREATE = 40,
        INVITE_UPDATE = 41,
        INVITE_DELETE = 42,
        WEBHOOK_CREATE = 50,
        WEBHOOK_UPDATE = 51,
        WEBHOOK_DELETE = 52,
        EMOJI_CREATE = 60,
        EMOJI_UPDATE = 61,
        EMOJI_DELETE = 62,
        MESSAGE_DELETE = 72
    }

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

    public enum PlayingType : int
    {
        Playing = 0,
        Streaming = 1,
        Listening = 2,
        Watching = 3
    }

    public enum CloseCode
    {
        GracefulClose = 1000,
        CloudflareLoadBalance = 1001,
        RandomServerError = 1006,
        UnknownError = 4000,
        InvalidPayloadSent = 4001,
        InvalidMessage = 4002,
        NoCurrentSessionSent = 4003,
        InvalidToken = 4004,
        AlreadyActiveSession = 4005,
        InvalidSequenceNumber = 4007,
        TooManyWSMessages = 4008,
        SessionTimedOut = 4009,
        InvalidShardData = 4010,
        TooManyGuilds = 4011
    }

    [Flags]
    public enum Permissions
    {
        CreateInstantInvite = 0x1,
        KickMembers = 0x2,
        BanMembers = 0x4,
        Administrator = 0x8,
        ManageChannels = 0x10,
        ManageGuild = 0x20,
        AddReactions = 0x40,
        ViewAuditLog = 0x80,
        ViewChannel = 0x400,
        SendMessages = 0x800,
        SendTTSMessages = 0x1000,
        ManageMessages = 0x2000,
        EmbedLinks = 0x4000,
        AttachFiles = 0x8000,
        ReadMessageHistory = 0x10000,
        MentionEveryone = 0x20000,
        UseExternalEmojis = 0x40000,
        Connect = 0x100000,
        Speak = 0x200000,
        MuteMembers = 0x400000,
        DeafenMembers = 0x800000,
        MoveMembers = 0x1000000,
        UseVad = 0x2000000,
        ChangeNickname = 0x4000000,
        ManageNicknames = 0x8000000,
        ManageRoles = 0x10000000,
        ManageWebhooks = 0x20000000,
        ManageEmojis = 0x40000000
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
        InvalidateSession = 9,
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
