using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using JetBrains.Annotations;

namespace GryphonUtilityBot.Web.Models;

[PublicAPI]
public sealed class Config : Configs.Config
{
    [Required]
    [MinLength(1)]
    public string PrimaryAgent { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string SecondaryAgent { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string PurchaseCurrency { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string ProductSoldNoteFormat { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string CultureInfoName { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string NotionToken { get; init; } = null!;

    public string? NotionWebhookSecret { get; init; }

    [Required]
    [MinLength(1)]
    public string NotionDatabaseId { get; init; } = null!;

    [Required]
    public DateOnly NotionStartWatchingDate { get; init; }

    [Required]
    [Range(double.Epsilon, double.MaxValue)]
    public double NotionPollsPerMinute { get; init; }

    [Required]
    [Range(double.Epsilon, double.MaxValue)]
    public double NotionUpdatesPerSecondLimit { get; init; }

    [Required]
    [MinLength(1)]
    public string GoogleCalendarId { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string GoogleCalendarColorId { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public Dictionary<string, string> RelevantProperties { get; init; } = null!;

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };
}