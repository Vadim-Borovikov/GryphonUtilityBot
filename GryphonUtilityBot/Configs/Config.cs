using AbstractBot.Models.Config;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;

namespace GryphonUtilityBot.Configs;

[PublicAPI]
public class Config : ConfigWithSheets
{
    [Required]
    [MinLength(1)]
    public string GoogleSheetIdArticles { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string GoogleTitleArticles { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string GoogleRangeArticles { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string GoogleSheetIdTimeline { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string GoogleTitleTimelineInput { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string GoogleTitleTimelineStreamlined { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string GoogleRangeTimeline { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string GoogleRangeTimelineClear { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string GoogleSheetIdTransactions { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string GoogleTitleTransactions { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string GoogleRangeTransactions { get; init; } = null!;

    [Required]
    public string DefaultCurrency { get; init; } = null!;

    [Required]
    public long TimelineChannelId { get; init; }

    [Required]
    public byte NotionConflictReties { get; init; }

    public Texts Texts { get; set; } = new();

    [Required]
    public double TimelineWriteIntervalSeconds { get; set; }
}