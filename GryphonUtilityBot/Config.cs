﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AbstractBot;
using JetBrains.Annotations;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace GryphonUtilityBot;

[PublicAPI]
public class Config : ConfigGoogleSheets
{
    [Required]
    [MinLength(1)]
    public string GoogleSheetId { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string GoogleRange { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string SavePath { get; init; } = null!;

    [Required]
    public long MistressId { get; init; }

    [Required]
    [MinLength(1)]
    public List<string> InsuranceMessageFormat { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string DefaultAddress { get; init; } = null!;

    [Required]
    public DateOnly ArrivalDate { get; init; }
}