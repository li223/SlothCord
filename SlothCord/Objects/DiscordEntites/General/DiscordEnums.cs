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
        GuildUpdate = 1,
        ChannelCreate = 10,
        ChannelUpdate = 11,
        ChannelDelete = 12,
        OverwriteCreate = 13,
        OverwriteUpdate = 14,
        OverwriteDelete = 15,
        MemberKick = 20,
        MemberPrune = 21,
        BanAdd = 22,
        BanRemove = 23,
        MemberUpdate = 24,
        MemberRolesUpdate = 25,
        RoleCreate = 30,
        RoleUpdate = 31,
        RoleDelete = 32,
        InviteCreate = 40,
        InviteUpdate = 41,
        InviteDelete = 42,
        WebhookCreate = 50,
        WebhookUpdate = 51,
        WebhookDelete = 52,
        EmojiCreate = 60,
        EmojiUpdate = 61,
        EmojiDelete = 62,
        MessageDelete = 72
    }

    public enum VerificationLevel
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        VeryHigh = 4
    }

    public enum MFALevel
    {
        None = 0,
        Elevated = 1
    }

    public enum ExplicitContentFilterLevel
    {
        Disabled = 0,
        MembersWithoutRoles = 1,
        AllMembers = 2
    }

    public enum NotificationLevel
    {
        AllMessages = 0,
        OnlyMentions = 1
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
        Streaming = 1,
        Listening = 2,
        Watching = 3
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

    public enum ChannelType
    {
        GuildText = 0,
        DirectMessage = 1,
        GuildVoice = 2
    }
}
