using AbstractBot.Interfaces.Modules;
using AbstractBot.Models.MessageTemplates;
using GoogleSheetsManager.Documents;
using GoogleSheetsManager.Extensions;
using GryphonUtilityBot.Configs;
using GryphonUtilityBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Articles;

internal sealed class Manager
{
    public Manager(Bot bot, Config config, ITextsProvider<Texts> textsProvider,
        GoogleSheetsManager.Documents.Manager documentsManager)
    {
        _bot = bot;
        _config = config;
        _textsProvider = textsProvider;
        _articles = new SortedSet<Article>();

        Dictionary<Type, Func<object?, object?>> additionalConverters = new()
        {
            { typeof(Uri), o => o.ToUri() }
        };
        additionalConverters[typeof(DateOnly)] = additionalConverters[typeof(DateOnly?)] =
            o => o.ToDateOnly(_bot.Core.Clock);

        GoogleSheetsManager.Documents.Document document = documentsManager.GetOrAdd(_config.GoogleSheetIdArticles);
        _sheet = document.GetOrAddSheet(_config.GoogleTitleArticles, additionalConverters);
    }


    public async Task ProcessNewArticleAsync(Chat chat, Article article)
    {
        await AddArticleAsync(article);

        Texts texts = _textsProvider.GetTextsFor(chat.Id);
        MessageTemplateText articleText = GetArticleMessageTemplate(article, texts);
        MessageTemplateText messageTemplate = _config.Texts.ArticleAddedFormat.Format(articleText);
        await messageTemplate.SendAsync(_bot.Core.UpdateSender, chat);
        await SendFirstArticleAsync(chat);
    }

    public async Task SendFirstArticleAsync(Chat chat)
    {
        await LoadAsync();

        Article? article = _articles.FirstOrDefault();
        Texts texts = _textsProvider.GetTextsFor(chat.Id);
        MessageTemplateText messageTemplate = article is null
            ? texts.NoMoreArticles
            : texts.ArticleWithNumberFormat.Format(_articles.Count, GetArticleMessageTemplate(article, texts));
        await messageTemplate.SendAsync(_bot.Core.UpdateSender, chat);
    }

    public async Task DeleteFirstArticleAsync(Chat chat)
    {
        await LoadAsync();

        Texts texts = _textsProvider.GetTextsFor(chat.Id);

        Article? article = _articles.FirstOrDefault();
        if (article is null)
        {
            await texts.AllArticlesDeletedAlready.SendAsync(_bot.Core.UpdateSender, chat);
            return;
        }

        _articles.Remove(article);
        Article? next = _articles.FirstOrDefault();
        if (next is not null)
        {
            next.Current = true;
        }
        await SaveAsync();

        MessageTemplateText articleText = GetArticleMessageTemplate(article, texts);
        MessageTemplateText messageTemplate = texts.ArticleDeletedFormat.Format(articleText);
        await messageTemplate.SendAsync(_bot.Core.UpdateSender, chat);
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
        List<Article> data = await _sheet.LoadAsync<Article>(_config.GoogleRangeArticles);
        _articles = new SortedSet<Article>(data);
    }

    private async Task SaveAsync()
    {
        await _sheet.ClearAsync(_config.GoogleRangeArticlesClear);
        await _sheet.SaveAsync(_config.GoogleRangeArticles, _articles.ToList());
    }

    private static MessageTemplateText GetArticleMessageTemplate(Article article, Texts texts)
    {
        string date = article.Date.ToString(texts.DateOnlyFormat);
        return texts.ArticleFormat.Format(date, article.Uri);
    }

    private SortedSet<Article> _articles;
    private readonly Bot _bot;
    private readonly Config _config;
    private readonly ITextsProvider<Texts> _textsProvider;
    private readonly Sheet _sheet;
}