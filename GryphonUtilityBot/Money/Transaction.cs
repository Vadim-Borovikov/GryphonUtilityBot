using GoogleSheetsManager;
using JetBrains.Annotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace GryphonUtilityBot.Money;

public abstract class Transaction
{
    [UsedImplicitly]
    [SheetField(ToTitle)]
    public string? To { get; set; }

    [UsedImplicitly]
    [Required]
    [SheetField(DateTitle, "{0:dd.MM.yyyy}")]
    public DateOnly Date;

    [UsedImplicitly]
    [Required]
    [SheetField(CurrencyTitle)]
    public string Currency { get; set; } = null!;

    [UsedImplicitly]
    [Required]
    [SheetField(AmountTitle)]
    public decimal Amount { get; set; }

    [UsedImplicitly]
    [SheetField(NoteTitle)]
    public string? Note;

    protected Transaction() { }

    protected Transaction(string? to, DateOnly date, decimal amount, string currency, string? note = null)
    {
        To = to;
        Date = date;
        Amount = amount;
        Currency = currency;
        Note = note;
    }

    private const string ToTitle = "Кому";
    private const string DateTitle = "Когда";
    private const string CurrencyTitle = "Чего";
    private const string AmountTitle = "Сколько";
    private const string NoteTitle = "Комментарий";
}