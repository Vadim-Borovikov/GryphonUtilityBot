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

public sealed class TransactionDebt : Transaction
{
    [UsedImplicitly]
    [Required]
    [SheetField(FromTitle)]
    public string From { get; set; } = null!;

    [UsedImplicitly]
    public TransactionDebt() { }

    private TransactionDebt(string from, string to, DateOnly date, decimal amount, string currency,
        string? note = null)
        : base(to, date, amount, currency, note)
    {
        From = from;
    }

    internal static TransactionDebt? TryParseReceipt(string s, DateOnly defaultDate, Texts texts, Clock clock,
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

        return new TransactionDebt(name, texts.Agents[partner].To, date, amount.Value, defaultCurrency, note);
    }

    private const string FromTitle = "Кто";
}