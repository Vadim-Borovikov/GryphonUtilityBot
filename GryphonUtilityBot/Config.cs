using System;
using AbstractBot;

namespace GryphonUtilityBot;

public sealed class Config : ConfigGoogleSheets
{
    internal readonly string GoogleRange;
    internal readonly string SavePath;
    internal readonly long MistressId;

    public Config(string token, string systemTimeZoneId, string dontUnderstandStickerFileId,
        string forbiddenStickerFileId, TimeSpan sendMessagePeriodPrivate, TimeSpan sendMessagePeriodGroup,
        TimeSpan sendMessagePeriodGlobal, string googleCredentialJson, string applicationName, string googleSheetId,
        string googleRange, string savePath, long mistressId)
        : base(token, systemTimeZoneId, dontUnderstandStickerFileId, forbiddenStickerFileId, sendMessagePeriodPrivate,
            sendMessagePeriodGroup, sendMessagePeriodGlobal, googleCredentialJson, applicationName, googleSheetId)
    {
        GoogleRange = googleRange;
        SavePath = savePath;
        MistressId = mistressId;
    }
}