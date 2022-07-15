using System;

namespace GryphonUtilityBot;

public sealed class Config : AbstractBot.Config
{
    internal readonly long MistressId;

    public Config(string token, string systemTimeZoneId, string dontUnderstandStickerFileId,
        string forbiddenStickerFileId, TimeSpan sendMessagePeriodPrivate, TimeSpan sendMessagePeriodGroup,
        TimeSpan sendMessagePeriodGlobal, long mistressId)
        : base(token, systemTimeZoneId, dontUnderstandStickerFileId,
            forbiddenStickerFileId, sendMessagePeriodPrivate, sendMessagePeriodGroup, sendMessagePeriodGlobal)
    {
        MistressId = mistressId;
    }
}