using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GryphonUtilityBot.Money;
using JetBrains.Annotations;

namespace GryphonUtilityBot.Web.Models;

[PublicAPI]
public sealed class Item
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public decimal Price { get; set; }

    [Required]
    public Dictionary<string, decimal> Shares { get; set; } = null!;

    public IEnumerable<TransactionDebt> GetTransactions(string mainFrom, string mainTo, string shareTo,
        string currency)
    {
        yield return new TransactionDebt
        {
            From = mainFrom,
            To = mainTo,
            Currency = currency,
            Amount = Price
        };

        foreach (string name in Shares.Keys)
        {
            yield return new TransactionDebt
            {
                From = name,
                Amount = Shares[name],
                To = shareTo,
                Currency = currency
            };
        }
    }
}