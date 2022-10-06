using System.ComponentModel.DataAnnotations;
using AbstractBot;
using JetBrains.Annotations;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace GryphonUtilityBot;

[PublicAPI]
public class Config : ConfigGoogleSheets
{
    [Required]
    public long MistressId { get; init; }

    [Required]
    public string GoogleRange { get; init; } = null!;
}