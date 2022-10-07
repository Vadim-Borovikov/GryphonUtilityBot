using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleSheetsManager;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Articles;

internal sealed class Manager
{
    public Manager(Bot bot)
    {
        _bot = bot;
        _articles = new SortedSet<Article>();
        GoogleSheetsManager.Utils.Converters[typeof(Uri)] = Utils.ToUri;
    }

    public static bool TryParseArticle(string? text, out Article? article)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            article = null;
            return false;
        }
        article = ParseArticle(text);
        return article is not null;
    }

    public async Task ProcessNewArticleAsync(Chat chat, Article article)
    {
        await AddArticleAsync(article);

        string articleText = GetArticleMessageText(article);
        await _bot.SendTextMessageAsync(chat, $"Добавлено: `{articleText}`\\.", ParseMode.MarkdownV2);
        await SendFirstArticleAsync(chat);
    }

    public async Task SendFirstArticleAsync(Chat chat)
    {
        await LoadAsync();

        string text = $"{_articles.Count}. {GetArticleMessageText(_articles.First())}";
        await _bot.SendTextMessageAsync(chat, text);
    }

    public async Task DeleteFirstArticleAsync(Chat chat)
    {
        await LoadAsync();

        Article article = _articles.First();
        _articles.Remove(article);

        await SaveAsync();

        string articleText = GetArticleMessageText(article);
        await _bot.SendTextMessageAsync(chat, $"Удалено: `{articleText}`\\.", ParseMode.MarkdownV2);
        await SendFirstArticleAsync(chat);
    }

    private static Article? ParseArticle(string text)
    {
        string[] parts = text.Split(' ');

        Uri? uri;
        switch (parts.Length)
        {
            case 1:
                uri = CreateUri(parts[0]);
                return uri is null ? null : new Article(DateTime.Today, uri);
            case 2:
                DateTime? date = ParseDate(parts[0]);
                if (!date.HasValue)
                {
                    return null;
                }
                uri = CreateUri(parts[1]);
                return uri is null ? null : new Article(date.Value, uri);
            default: return null;
        }
    }

    private static DateTime? ParseDate(string dateString)
    {
        if (DateTime.TryParse(dateString, out DateTime date))
        {
            return date;
        }

        if (!int.TryParse(dateString, out int day))
        {
            return null;
        }

        try
        {
            return new DateTime(DateTime.Today.Year, DateTime.Today.Month, day);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static Uri? CreateUri(string uriString)
    {
        return Uri.TryCreate(uriString, UriKind.Absolute, out Uri? uri) ? uri : null;
    }

    private async Task AddArticleAsync(Article article)
    {
        await LoadAsync();

        _articles.Add(article);

        await SaveAsync();
    }

    private async Task LoadAsync()
    {
        SheetData<Article> data =
            await DataManager.GetValuesAsync<Article>(_bot.GoogleSheetsProvider, _bot.Config.GoogleRange);
        _articles = new SortedSet<Article>(data.Instances);
        _titles = data.Titles;
    }

    private async Task SaveAsync()
    {
        await _bot.GoogleSheetsProvider.ClearValuesAsync(_bot.Config.GoogleRange);
        SheetData<Article> data = new(_articles.ToList(), _titles);
        await DataManager.UpdateValuesAsync(_bot.GoogleSheetsProvider, _bot.Config.GoogleRange, data);
    }

    private static string GetArticleMessageText(Article article)
    {
        return $"{article.Date:d MMMM yyyy}{Environment.NewLine}{article.Uri}";
    }

    private SortedSet<Article> _articles;
    private readonly Bot _bot;
    private IList<string> _titles = Array.Empty<string>();
}