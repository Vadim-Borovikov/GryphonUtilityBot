using System;
using System.Collections.Generic;
using System.Linq;
using GoogleSheetsManager;
using GoogleSheetsManager.Extensions;
using GryphonUtilities.Time;
using JetBrains.Annotations;

namespace GryphonUtilityBot.Money;

public sealed class TransactionExpense : Transaction
{
    [UsedImplicitly]
    [SheetField(CategoryTitle)]
    public string? Category { get; set; }

    [UsedImplicitly]
    [SheetField(SmsNameTitle)]
    public string? SmsName { get; set; }

    [UsedImplicitly]
    [SheetField(CityTitle)]
    public string? City { get; set; }

    [UsedImplicitly]
    public TransactionExpense() { }

    private TransactionExpense(string? category, string? to, DateOnly date, decimal amount, string currency,
        string city, string? note = null, string? smsName = null)
        : base(to, date, amount, currency, note)
    {
        Category = category;
        SmsName = smsName;
        City = city;
    }

    internal void RegisterData()
    {
        if (!string.IsNullOrWhiteSpace(Category))
        {
            Categories.Add(Category);
        }

        if (string.IsNullOrWhiteSpace(To))
        {
            return;
        }

        Venues.TryAdd(To, new HashSet<string>());
        if (!string.IsNullOrWhiteSpace(Category))
        {
            Venues[To].Add(Category);
        }
        if (!string.IsNullOrWhiteSpace(SmsName))
        {
            SmsNames.TryAdd(SmsName, To);
        }
    }

    internal static TransactionExpense? TryParseReceipt(string s, DateOnly defaultDate, Clock clock,
        string defaultCurrency, string defaultCity)
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
        if (parts.Count <= index)
        {
            return null;
        }

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

        string? venue = null;

        if (Categories.TryGetValue(parts[index], out string? category))
        {
            ++index;
        }
        else if (Venues.ContainsKey(parts[index]))
        {
            venue = parts[index];
            category = GetCategoryIfSingle(venue);
            ++index;
        }

        string note = string.Join(" ", parts.Skip(index));

        return new TransactionExpense(category, venue, date, amount.Value, defaultCurrency, defaultCity, note);
    }

    internal static TransactionExpense? TryParseSms(string s, Clock clock, string defaultCurrency, string defaultCity,
        string smsSeparator)
    {
        List<string> parts = s.Split(smsSeparator).Where(p => p.Length > 0).ToList();

        byte index = 0;
        if (parts.Count <= index)
        {
            return null;
        }

        List<string> subParts = parts[index].Split(null).Where(p => p.Length > 0).ToList();
        string? amountPart = subParts.LastOrDefault()?.Replace("RSD", "");
        decimal? amount = amountPart.ToDecimal();
        if (amount is null)
        {
            return null;
        }

        ++index;
        if (parts.Count <= index)
        {
            return null;
        }

        string smsName = parts[index].Replace("mesto ", "");
        string? category = null;
        if (SmsNames.TryGetValue(smsName, out string? venue))
        {
            category = GetCategoryIfSingle(venue);
        }

        ++index;
        if (parts.Count <= index)
        {
            return null;
        }

        string datePart = parts[index].Replace("dana ", "");
        subParts = datePart.Split(null).Where(p => p.Length > 0).ToList();
        DateOnly? date = subParts.FirstOrDefault().ToDateOnly(clock);
        return date.HasValue
            ? new TransactionExpense(category, venue, date.Value, amount.Value, defaultCurrency, defaultCity, null,
                smsName)
            : null;
    }

    private static string? GetCategoryIfSingle(string venue)
    {
        return Venues.TryGetValue(venue, out HashSet<string>? categories) && (categories.Count == 1)
            ? categories.Single()
            : null;
    }

    private const string CategoryTitle = "Зачем";
    private const string SmsNameTitle = "SMS Name";
    private const string CityTitle = "Город";

    internal static readonly HashSet<string> Categories = new();
    internal static readonly Dictionary<string, HashSet<string>> Venues = new();
    internal static readonly Dictionary<string, string> SmsNames = new();
}