using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleSheetsManager.Documents;
using GoogleSheetsManager.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Articles;

internal sealed class Manager
{
    public Manager(Bot bot, GoogleSheetsManager.Documents.Manager documentsManager)
    {
        _bot = bot;
        _articles = new SortedSet<Article>();

        Dictionary<Type, Func<object?, object?>> additionalConverters = new()
        {
            { typeof(Uri), o => o.ToUri() }
        };
        additionalConverters[typeof(DateOnly)] = additionalConverters[typeof(DateOnly?)] =
            o => o.ToDateOnly(_bot.Clock);

        GoogleSheetsManager.Documents.Document document = documentsManager.GetOrAdd(_bot.Config.GoogleSheetId);
        _sheet = document.GetOrAddSheet(bot.Config.GoogleTitle, additionalConverters);
    }


    public async Task ProcessNewArticleAsync(Chat chat, Article article)
    {
        await AddArticleAsync(article);

        string articleText = GetArticleMessageText(article);
        await _bot.SendTextMessageAsync(chat, $"Добавлено: `{articleText}`\\.", parseMode: ParseMode.MarkdownV2);
        await SendFirstArticleAsync(chat);
    }

    public async Task SendFirstArticleAsync(Chat chat)
    {
        await LoadAsync();

        Article? article = _articles.FirstOrDefault();
        string text = article is null ? "Больше статей нет." : $"{_articles.Count}. {GetArticleMessageText(article)}";
        await _bot.SendTextMessageAsync(chat, text);
    }

    public async Task DeleteFirstArticleAsync(Chat chat)
    {
        await LoadAsync();

        Article? article = _articles.FirstOrDefault();
        if (article is null)
        {
            await _bot.SendTextMessageAsync(chat, "Список статей уже пуст.");
            return;
        }

        _articles.Remove(article);
        Article? next = _articles.FirstOrDefault();
        if (next is not null)
        {
            next.Current = true;
        }
        await SaveAsync();

        string articleText = GetArticleMessageText(article);
        await _bot.SendTextMessageAsync(chat, $"Удалено: `{articleText}`\\.", parseMode: ParseMode.MarkdownV2);
        await SendFirstArticleAsync(chat);
    }

    private async Task AddArticleAsync(Article article)
    {
        await LoadAsync();

        if (_articles.Count == 0)
        {
            article.Current = true;
        }
        _articles.Add(article);

        await SaveAsync();
    }

    private async Task LoadAsync()
    {
        List<Article> data = await _sheet.LoadAsync<Article>(_bot.Config.GoogleRange);
        _articles = new SortedSet<Article>(data);
    }

    private async Task SaveAsync()
    {
        await _sheet.ClearAsync(_bot.Config.GoogleRange);
        await _sheet.SaveAsync(_bot.Config.GoogleRange, _articles.ToList());
    }

    private static string GetArticleMessageText(Article article)
    {
        return $"{article.Date:d MMMM yyyy}{Environment.NewLine}{article.Uri}";
    }

    private SortedSet<Article> _articles;
    private readonly Bot _bot;
    private readonly Sheet _sheet;
}