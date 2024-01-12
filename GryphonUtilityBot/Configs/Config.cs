using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AbstractBot.Configs;
using JetBrains.Annotations;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace GryphonUtilityBot.Configs;

[PublicAPI]
public class Config : ConfigWithSheets<Texts>
{
    [Required]
    [MinLength(1)]
    public string GoogleSheetId { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string GoogleTitle { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string GoogleRange { get; init; } = null!;

    [Required]
    public string DefaultCurrency { get; init; } = null!;

    [Required]
    public long ItemVendorId { get; init; }

    [Required]
    public Dictionary<byte, Product> Products { get; init; } = null!;
}