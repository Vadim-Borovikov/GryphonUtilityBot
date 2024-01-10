using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GoogleSheetsManager;
using GryphonUtilityBot.Configs;
using JetBrains.Annotations;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace GryphonUtilityBot.Money;

internal sealed class Transaction
{
    [UsedImplicitly]
    [Required]
    [SheetField(FromTitle)]
    public readonly string From;

    [UsedImplicitly]
    [Required]
    [SheetField(ToTitle)]
    public readonly string To;

    [UsedImplicitly]
    [Required]
    [SheetField(DateTitle, "{0:dd.MM.yyyy}")]
    public readonly DateOnly Date;

    [UsedImplicitly]
    [Required]
    [SheetField(CurrencyTitle)]
    public readonly string Currency;

    [UsedImplicitly]
    [Required]
    [SheetField(AmountTitle)]
    public readonly decimal Amount;

    [UsedImplicitly]
    [Required]
    [SheetField(NoteTitle)]
    public readonly string? Note;

    private Transaction(string from, string to, DateOnly date, decimal amount, string currency, string? note = null)
    {
        From = from;
        To = to;
        Date = date;
        Amount = amount;
        Currency = currency;
        Note = note;
    }

    public static Transaction? TryParseReceipt(string s, DateOnly defaultDate, Texts texts)
    {
        string[] parts = s.Split(null);

        int index = 0;
        if (parts.Length <= index)
        {
            return null;
        }

        string tag = parts[index];

        string from;
        string to;

        if (tag.Equals(texts.TagDima, StringComparison.CurrentCultureIgnoreCase))
        {
            from = texts.FromDima;
            to = texts.ToRita;
        }
        else if (tag.Equals(texts.TagRita, StringComparison.CurrentCultureIgnoreCase))
        {
            from = texts.FromRita;
            to = texts.ToDima;
        }
        else
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

        return new Transaction(from, to, date, amount, texts.DefaultCurrency, note);
    }

    private const string FromTitle = "Кто";
    private const string ToTitle = "Кому";
    private const string DateTitle = "Когда";
    private const string CurrencyTitle = "Чего";
    private const string AmountTitle = "Сумма напрямую";
    private const string NoteTitle = "Комментарий";
}