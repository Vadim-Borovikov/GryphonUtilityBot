using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GoogleSheetsManager;
using GryphonUtilityBot.Configs;
using JetBrains.Annotations;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace GryphonUtilityBot.Money;

public sealed class Transaction
{
    [UsedImplicitly]
    [Required]
    [SheetField(FromTitle)]
    public string From { get; set; } = null!;

    [UsedImplicitly]
    [Required]
    [SheetField(ToTitle)]
    public string To { get; set; } = null!;

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
    [Required]
    [SheetField(NoteTitle)]
    public string? Note;

    [UsedImplicitly]
    public Transaction() { }

    private Transaction(string from, string to, DateOnly date, decimal amount, string currency, string? note = null)
    {
        From = from;
        To = to;
        Date = date;
        Amount = amount;
        Currency = currency;
        Note = note;
    }

    internal static Transaction? TryParseReceipt(string s, DateOnly defaultDate, Texts texts, string defaultCurrency)
    {
        string[] parts = s.Split(null);

        int index = 0;
        if (parts.Length <= index)
        {
            return null;
        }

        string tag = parts[index];

        string? name = texts.TryGetAgent(tag);
        if (name is null)
        {
            return null;
        }

        string? partner = texts.TryGetPartner(texts.Agents[name]);
        if (partner is null)
        {
            return null;
        }

        ++index;
        if (parts.Length <= index)
        {
            return null;
        }

        DateOnly date = defaultDate;
        if (DateOnly.TryParse(parts[index], out DateOnly result))
        {
            date = result;
            ++index;
            if (parts.Length <= index)
            {
                return null;
            }
        }

        if (!decimal.TryParse(parts[index], out decimal amount))
        {
            return null;
        }

        string note = string.Join(" ", parts.Skip(index + 1));

        return new Transaction(name, texts.Agents[partner].To, date, amount, defaultCurrency, note);
    }

    private const string FromTitle = "Кто"; private const string ToTitle = "Кому";
    private const string DateTitle = "Когда";
    private const string CurrencyTitle = "Чего";
    private const string AmountTitle = "Сумма напрямую";
    private const string NoteTitle = "Комментарий";
}