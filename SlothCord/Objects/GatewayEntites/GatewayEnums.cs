using System;
using System.Collections.Generic;
using System.Text;

namespace SlothCord.Objects
{
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

    public enum DispatchType
    {
        Ready = 0,
        Presenceupdate = 1,
        GuildCreate = 2,
        MessageCreate = 3,
        TypingStart = 4,
        ChannelCreate = 5,
        ChannelUpdate = 6,
        ChannelDelete = 7,
        PinsUpdate = 8,
        GuildUpdate = 9,
        GuildDelete = 10,
        BanAdd = 11,
        BanRemove = 12,
        EmojiUpdate = 13,
        IntergrationUpdate = 14,
        MemberUpdate = 15,
        MemberAdd = 16,
        MemberRemoved = 17,
        MembersChunk = 18,
        RoleCreate = 19,
        RoleDelete = 20,
        RoleUpdate = 21,
        MessageUpdate = 22,
        MessageDelete = 23,
        BulkDelete = 24,
        ReactionAdd = 25,
        ReactionRemove = 26,
        ReactionRemoveAll = 27,
        UserUpdate = 28,
        VoicStateUpdate = 29,
        VoiceServerUpdate = 30,
        WebhookUpdate = 31
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
}
