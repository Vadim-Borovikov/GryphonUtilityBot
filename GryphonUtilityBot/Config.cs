using System;

namespace GryphonUtilityBot;

public sealed class Config : AbstractBot.Config
{
    internal readonly string SavePath;
    internal readonly long MistressId;

    public Config(string token, string systemTimeZoneId, string dontUnderstandStickerFileId,
        string forbiddenStickerFileId, TimeSpan sendMessagePeriodPrivate, TimeSpan sendMessagePeriodGroup,
        TimeSpan sendMessagePeriodGlobal, string savePath, long mistressId)
        : base(token, systemTimeZoneId, dontUnderstandStickerFileId, forbiddenStickerFileId, sendMessagePeriodPrivate,
            sendMessagePeriodGroup, sendMessagePeriodGlobal)
    {
        SavePath = savePath;
        MistressId = mistressId;
    }
}