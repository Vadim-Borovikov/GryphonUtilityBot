using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GoogleSheetsManager;
using GoogleSheetsManager.Extensions;
using GryphonUtilities.Time;
using JetBrains.Annotations;

namespace GryphonUtilityBot.Money;

public sealed class TransactionExpense : Transaction
{
    [UsedImplicitly]
    [Required]
    [SheetField(CategoryTitle)]
    public string Category { get; set; } = null!;

    [UsedImplicitly]
    public TransactionExpense() { }

    private TransactionExpense(string category, string to, DateOnly date, decimal amount, string currency,
        string? note = null)
        : base(to, date, amount, currency, note)
    {
        Category = category;
    }

    internal static Transaction? TryParseReceipt(string s, DateOnly defaultDate, Clock clock,
        string defaultCurrency)
    {
        List<string> parts = s.Split(null).Where(p => p.Length > 0).ToList();

        int index = 0;
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

        string where = parts[index];
        string category = string.Empty;
        string to = string.Empty;

        if (Categories.Contains(where))
        {
            category = where;
            ++index;
        }
        else if (Places.TryGetValue(where, out string? placeCategory))
        {
            to = where;
            category = placeCategory;
            ++index;
        }

        string note = string.Join(" ", parts.Skip(index));

        return new TransactionExpense(category, to, date, amount.Value, defaultCurrency, note);
    }

    private const string CategoryTitle = "Зачем";

    internal static readonly HashSet<string> Categories = new();
    internal static readonly Dictionary<string, string> Places = new();
}