using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GryphonUtilityBot.Money;
using JetBrains.Annotations;

namespace GryphonUtilityBot.Configs;

[PublicAPI]
public class Product
{
    [Required]
    [MinLength(1)]
    public string Name { get; set; } = null!;

    [Required]
    public List<Transaction> Transactions { get; init; } = null!;
}