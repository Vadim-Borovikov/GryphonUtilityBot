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

    internal TransactionDebt(string from, string to, DateOnly date, decimal amount, string currency,
        string? note = null)
        : base(to, date, amount, currency, note)
    {
        From = from;
    }

    private const string FromTitle = "Кто";
}