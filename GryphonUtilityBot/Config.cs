using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace GryphonUtilityBot;

[PublicAPI]
public class Config : AbstractBot.Config
{
    [Required]
    public long MistressId { get; init; }

    [Required]
    [MinLength(1)]
    public List<string> InsuranceMessageFormatLines { get; init; } = null!;

    internal string InsuranceMessageFormat => string.Join(Environment.NewLine, InsuranceMessageFormatLines);

    [Required]
    [MinLength(1)]
    public string DefaultAddress { get; init; } = null!;

    [Required]
    public DateOnly ArrivalDate { get; init; }
}