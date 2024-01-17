using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;

// ReSharper disable NullableWarningSuppressionIsUsed

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
}