using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbstractBot.Configs.MessageTemplates;
using GoogleSheetsManager.Documents;
using GoogleSheetsManager.Extensions;
using GryphonUtilityBot.Extensions;
using Telegram.Bot.Types;

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

        GoogleSheetsManager.Documents.Document document = documentsManager.GetOrAdd(_bot.Config.GoogleSheetIdArticles);
        _sheet = document.GetOrAddSheet(bot.Config.GoogleTitleArticles, additionalConverters);
    }


    public async Task ProcessNewArticleAsync(Chat chat, Article article)
    {
        await AddArticleAsync(article);

        MessageTemplateText articleText = GetArticleMessageTemplate(article);
        MessageTemplateText messageTemplate = _bot.Config.Texts.ArticleAddedFormat.Format(articleText);
        await messageTemplate.SendAsync(_bot, chat);
        await SendFirstArticleAsync(chat);
    }

    public async Task SendFirstArticleAsync(Chat chat)
    {
        await LoadAsync();

        Article? article = _articles.FirstOrDefault();
        MessageTemplateText messageTemplate = article is null
            ? _bot.Config.Texts.NoMoreArticles
            : _bot.Config.Texts.ArticleWithNumberFormat.Format(_articles.Count, GetArticleMessageTemplate(article));
        await messageTemplate.SendAsync(_bot, chat);
    }

    public async Task DeleteFirstArticleAsync(Chat chat)
    {
        await LoadAsync();

        Article? article = _articles.FirstOrDefault();
        if (article is null)
        {
            await _bot.Config.Texts.AllArticlesDeletedAlready.SendAsync(_bot, chat);
            return;
        }

        _articles.Remove(article);
        Article? next = _articles.FirstOrDefault();
        if (next is not null)
        {
            next.Current = true;
        }
        await SaveAsync();

        MessageTemplateText articleText = GetArticleMessageTemplate(article);
        MessageTemplateText messageTemplate = _bot.Config.Texts.ArticleDeletedFormat.Format(articleText);
        await messageTemplate.SendAsync(_bot, chat);
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
        List<Article> data = await _sheet.LoadAsync<Article>(_bot.Config.GoogleRangeArticles);
        _articles = new SortedSet<Article>(data);
    }

    private async Task SaveAsync()
    {
        await _sheet.ClearAsync(_bot.Config.GoogleRangeArticles);
        await _sheet.SaveAsync(_bot.Config.GoogleRangeArticles, _articles.ToList());
    }

    private MessageTemplateText GetArticleMessageTemplate(Article article)
    {
        string date = article.Date.ToString(_bot.Config.Texts.DateOnlyFormat);
        return _bot.Config.Texts.ArticleFormat.Format(date, article.Uri);
    }

    private SortedSet<Article> _articles;
    private readonly Bot _bot;
    private readonly Sheet _sheet;
}