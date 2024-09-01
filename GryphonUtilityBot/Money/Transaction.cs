using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GoogleSheetsManager;
using GoogleSheetsManager.Extensions;
using GryphonUtilities.Time;
using GryphonUtilityBot.Configs;
using JetBrains.Annotations;

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

    internal static Transaction? TryParseReceipt(string s, DateOnly defaultDate, Texts texts, Clock clock,
        string defaultCurrency)
    {
        List<string> parts = s.Split(null).Where(p => p.Length > 0).ToList();

        int index = 0;
        if (parts.Count <= index)
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
        if (parts.Count <= index)
        {
            return null;
        }

        decimal? amount = parts[index].ToDecimal();
        if (amount is null)
        {
            return null;
        }
        ++index;

        DateOnly date = defaultDate;
        DateOnly? result = parts[index].ToDateOnly(clock);
        if (result.HasValue)
        {
            date = result.Value;
            ++index;
            if (parts.Count <= index)
            {
                return null;
            }
        }

        string note = string.Join(" ", parts.Skip(index));

        return new Transaction(name, texts.Agents[partner].To, date, amount.Value, defaultCurrency, note);
    }

    private const string FromTitle = "Кто";
    private const string ToTitle = "Кому";
    private const string DateTitle = "Когда";
    private const string CurrencyTitle = "Чего";
    private const string AmountTitle = "Сколько";
    private const string NoteTitle = "Комментарий";
}