﻿using System;
using System.Collections.Generic;
using System.Linq;
using GoogleSheetsManager;
using GryphonUtilities;
using Newtonsoft.Json;

namespace GryphonUtilityBot.Web.Models;

public sealed class ConfigJson : IConvertibleTo<Config>
{
    [JsonProperty]
    public string? Token { get; set; }
    [JsonProperty]
    public string? SystemTimeZoneId { get; set; }
    [JsonProperty]
    public string? DontUnderstandStickerFileId { get; set; }
    [JsonProperty]
    public string? ForbiddenStickerFileId { get; set; }
    [JsonProperty]
    public double? UpdatesPerSecondLimitPrivate { get; set; }
    [JsonProperty]
    public double? UpdatesPerMinuteLimitGroup { get; set; }
    [JsonProperty]
    public double? UpdatesPerSecondLimitGlobal { get; set; }

    [JsonProperty]
    public string? Host { get; set; }
    [JsonProperty]
    public List<string?>? About { get; set; }
    [JsonProperty]
    public List<string?>? ExtraCommands { get; set; }
    [JsonProperty]
    public List<long?>? AdminIds { get; set; }
    [JsonProperty]
    public long? SuperAdminId { get; set; }

    [JsonProperty]
    public string? GoogleCredentialJson { get; set; }
    [JsonProperty]
    public string? ApplicationName { get; set; }
    [JsonProperty]
    public string? GoogleSheetId { get; set; }

    [JsonProperty]
    public string? GoogleRange { get; set; }
    [JsonProperty]
    public string? SavePath { get; set; }
    [JsonProperty]
    public long? MistressId { get; set; }

    [JsonProperty]
    public string? AdminIdsJson { get; set; }
    [JsonProperty]
    public string? CultureInfoName { get; set; }
    [JsonProperty]
    public Dictionary<string, string?>? GoogleCredential { get; set; }

    [JsonProperty]
    public List<Uri?>? PingUrls { get; set; }
    [JsonProperty]
    public string? PingUrlsJson { get; set; }

    public Config Convert()
    {
        string token = Token.GetValue(nameof(Token));
        string systemTimeZoneId = SystemTimeZoneId.GetValue(nameof(SystemTimeZoneId));
        string dontUnderstandStickerFileId = DontUnderstandStickerFileId.GetValue(nameof(DontUnderstandStickerFileId));
        string forbiddenStickerFileId = ForbiddenStickerFileId.GetValue(nameof(ForbiddenStickerFileId));

        double updatesPerSecondLimitPrivate =
            UpdatesPerSecondLimitPrivate.GetValue(nameof(UpdatesPerSecondLimitPrivate));
        TimeSpan sendMessagePeriodPrivate = TimeSpan.FromSeconds(1.0 / updatesPerSecondLimitPrivate);

        double updatesPerMinuteLimitGroup = UpdatesPerMinuteLimitGroup.GetValue(nameof(UpdatesPerMinuteLimitGroup));
        TimeSpan sendMessagePeriodGroup = TimeSpan.FromMinutes(1.0 / updatesPerMinuteLimitGroup);

        double updatesPerSecondLimitGlobal = UpdatesPerSecondLimitGlobal.GetValue(nameof(UpdatesPerSecondLimitGlobal));
        TimeSpan sendMessagePeriodGlobal = TimeSpan.FromSeconds(1.0 / updatesPerSecondLimitGlobal);

        string googleCredentialJson = string.IsNullOrWhiteSpace(GoogleCredentialJson)
            ? JsonConvert.SerializeObject(GoogleCredential)
            : GoogleCredentialJson;
        string applicationName = ApplicationName.GetValue(nameof(ApplicationName));
        string googleSheetId = GoogleSheetId.GetValue(nameof(GoogleSheetId));

        string googleRange = GoogleRange.GetValue(nameof(GoogleRange));
        string savePath = SavePath.GetValue(nameof(SavePath));
        long mistressId = MistressId.GetValue(nameof(MistressId));

        if (AdminIds is null || (AdminIds.Count == 0))
        {
            string json = AdminIdsJson.GetValue(nameof(AdminIdsJson));
            AdminIds = JsonConvert.DeserializeObject<List<long?>>(json);
        }

        return new Config(token, systemTimeZoneId, dontUnderstandStickerFileId, forbiddenStickerFileId,
            sendMessagePeriodPrivate, sendMessagePeriodGroup, sendMessagePeriodGlobal, googleCredentialJson,
            applicationName, googleSheetId, googleRange, savePath, mistressId)
        {
            Host = Host,
            About = About is null ? null : string.Join(Environment.NewLine, About),
            ExtraCommands = ExtraCommands is null ? null : string.Join(Environment.NewLine, ExtraCommands),
            AdminIds = AdminIds?.Select(id => id.GetValue("Admin id")).ToList(),
            SuperAdminId = SuperAdminId
        };
    }
}