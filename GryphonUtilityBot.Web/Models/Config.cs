﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace GryphonUtilityBot.Web.Models;

[PublicAPI]
public sealed class Config : GryphonUtilityBot.Config
{
    [Required]
    [MinLength(1)]
    public string CultureInfoName { get; init; } = null!;

    public List<Uri?>? PingUrls { get; init; }

    public string? PingUrlsJson { get; init; }

    [Required]
    [MinLength(1)]
    public string NotionToken { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string NotionDatabaseId { get; init; } = null!;

    [Required]
    public DateTime NotionStartWatchingDate { get; init; }

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
}