using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AbstractBot.Configs.MessageTemplates;
using JetBrains.Annotations;

namespace GryphonUtilityBot.Configs;

[PublicAPI]
public class Texts : AbstractBot.Configs.Texts
{
    [Required]
    public MessageTemplateText RecordAdded { get; init; } = null!;

    [Required]
    public MessageTemplateText RecordUpdated { get; init; } = null!;

    [Required]
    public MessageTemplateText RecordNotFound { get; init; } = null!;

    [Required]
    public MessageTemplateText RecordsNotFound { get; init; } = null!;

    [Required]
    public MessageTemplateText ForbiddenForMistress { get; init; } = null!;

    [Required]
    public MessageTemplateText ArticleAddedFormat { get; init; } = null!;

    [Required]
    public MessageTemplateText ArticleDeletedFormat { get; init; } = null!;

    [Required]
    public MessageTemplateText NoMoreArticles { get; init; } = null!;

    [Required]
    public MessageTemplateText AllArticlesDeletedAlready { get; init; } = null!;

    [Required]
    public MessageTemplateText ArticleFormat { get; init; } = null!;

    [Required]
    public MessageTemplateText ArticleWithNumberFormat { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string ArticleCommandDescription { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string ReadCommandDescription { get; init; } = null!;

    [Required]
    public MessageTemplateText AddReceiptDescription { get; init; } = null!;

    [Required]
    public MessageTemplateText AddArticleDescription { get; init; } = null!;

    [Required]
    public MessageTemplateText AddRecordDescription { get; init; } = null!;

    [Required]
    public MessageTemplateText FindRecordDescription { get; init; } = null!;

    [Required]
    public MessageTemplateText RememberTagDescription { get; init; } = null!;

    [Required]
    public MessageTemplateText TagRecordDescription { get; init; } = null!;

    [Required]
    public MessageTemplateText TransactionAddedFormat { get; init; } = null!;

    [Required]
    public MessageTemplateText TransactionCoreFormat { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string DateOnlyFormat { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public Dictionary<string, Agent> Agents { get; init; } = null!;

    [Required]
    public MessageTemplateText ListItemFormat { get; init; } = null!;

    public string? TryGetAgent(string tag)
    {
        return Agents.Keys.SingleOrDefault(n => tag.Equals(Agents[n].Tag, StringComparison.CurrentCultureIgnoreCase));
    }

    public string? TryGetPartner(Agent agent)
    {
        return Agents.Keys.SingleOrDefault(n => n.Equals(agent.Partner, StringComparison.CurrentCultureIgnoreCase));
    }
}