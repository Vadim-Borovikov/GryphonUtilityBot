using System.ComponentModel.DataAnnotations;
using AbstractBot.Configs;
using JetBrains.Annotations;

namespace GryphonUtilityBot.Configs;

[PublicAPI]
public class Config : ConfigWithSheets<Texts>
{
    [Required]
    [MinLength(1)]
    public string GoogleSheetIdArticles { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string GoogleTitleArticles { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string GoogleRangeArticles { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string GoogleRangeArticlesClear { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string GoogleSheetIdTransactions { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string GoogleTitleTransactions { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string GoogleRangeTransactions { get; init; } = null!;

    [Required]
    public string DefaultCurrency { get; init; } = null!;

    [Required]
    public long TransactionLogsChatId { get; init; }

    [Required]
    public byte NotionConflictReties { get; init; }
}