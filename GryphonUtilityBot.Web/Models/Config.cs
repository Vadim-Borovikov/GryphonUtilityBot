using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
// ReSharper disable NullableWarningSuppressionIsUsed

namespace GryphonUtilityBot.Web.Models;

[PublicAPI]
public sealed class Config : GryphonUtilityBot.Config
{
    [Required]
    [MinLength(1)]
    public string CultureInfoName { get; init; } = null!;

    public List<Uri>? PingUrls { get; init; }
    public string? PingUrlsJson { get; init; }
}