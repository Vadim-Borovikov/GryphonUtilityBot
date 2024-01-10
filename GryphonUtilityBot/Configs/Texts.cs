using System.ComponentModel.DataAnnotations;
using AbstractBot.Configs;
using JetBrains.Annotations;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace GryphonUtilityBot.Configs;

[PublicAPI]
public class Texts : AbstractBot.Configs.Texts
{
    [Required]
    public MessageTemplate AddReceiptDescription { get; init; } = null!;

    [Required]
    public MessageTemplate TransactionAddedFormat { get; init; } = null!;

    [Required]
    public string DateOnlyFormat { get; init; } = null!;

    [Required]
    public string TagDima { get; init; } = null!;
    [Required]
    public string TagRita { get; init; } = null!;

    [Required]
    public string FromDima { get; init; } = null!;
    [Required]
    public string FromRita { get; init; } = null!;

    [Required]
    public string VerbDima { get; init; } = null!;
    [Required]
    public string VerbRita { get; init; } = null!;

    [Required]
    public string ToDima { get; init; } = null!;
    [Required]
    public string ToRita { get; init; } = null!;

    [Required]
    public string DefaultCurrency { get; init; } = null!;
}