using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace GryphonUtilityBot.Web.Models;

[PublicAPI]
public sealed class Purchase
{
    [Required]
    public string ClientName { get; set; } = null!;

    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public List<Item> Items { get; set; } = null!;
}