using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
    public MessageTemplate TransactionCoreFormat { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string DateOnlyFormat { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public Dictionary<string, Agent> Agents { get; init; } = null!;

    [Required]
    public MessageTemplate PaymentConfirmationFormat { get; init; } = null!;

    [Required]
    public MessageTemplate ListItemFormat { get; init; } = null!;

    public string? TryGetAgent(string tag)
    {
        return Agents.Keys.SingleOrDefault(n => tag.Equals(Agents[n].Tag, StringComparison.CurrentCultureIgnoreCase));
    }

    public string? TryGetPartner(Agent agent)
    {
        return Agents.Keys.SingleOrDefault(n => n.Equals(agent.Partner, StringComparison.CurrentCultureIgnoreCase));
    }
}