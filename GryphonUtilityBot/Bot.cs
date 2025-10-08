using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractBot.Bots;
using AbstractBot.Operations.Data;
using GryphonUtilityBot.Configs;
using GryphonUtilityBot.Money;
using GryphonUtilityBot.Operations;
using GryphonUtilityBot.Operations.Commands;
using JetBrains.Annotations;

namespace GryphonUtilityBot;

public sealed class Bot : BotWithSheets<Config, Texts, object, CommandDataSimple>
{
    [Flags]
    internal enum AccessType
    {
        [UsedImplicitly]
        Default = 1,
        Admin = 2
    }

    public Bot(Config config) : base(config)
    {
        Articles.Manager articlesManager = new(this, DocumentsManager);

        _financemanager = new Manager(this, DocumentsManager);
        Operations.Add(new AddReceipt(this, _financemanager));

        Operations.Add(new ArticleCommand(this, articlesManager));
        Operations.Add(new ReadCommand(this, articlesManager));

        Operations.Add(new AddArticle(this, articlesManager));
    }

    public Task AddSimultaneousTransactionsAsync(List<Transaction> transactions, DateOnly date, string note)
    {
        return _financemanager.AddSimultaneousTransactionsAsync(transactions, date, note);
    }

    private readonly Manager _financemanager;
}