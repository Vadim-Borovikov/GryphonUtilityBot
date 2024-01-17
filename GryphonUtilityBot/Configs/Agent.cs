using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace GryphonUtilityBot.Configs;

[PublicAPI]
public class Agent
{
    [Required]
    public string? Partner { get; init; }
    [Required]
    public string Verb { get; init; } = null!;
    [Required]
    public string To { get; init; } = null!;

    public string? Tag { get; init; }
}