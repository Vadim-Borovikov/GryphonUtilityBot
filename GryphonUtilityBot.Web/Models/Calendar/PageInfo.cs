﻿using Notion.Client;
using System.Collections.Generic;
using System;
using System.Linq;
using GryphonUtilities.Time;

namespace GryphonUtilityBot.Web.Models.Calendar;

internal sealed class PageInfo
{
    public readonly Page Page;
    public readonly string Title;
    public readonly (DateTimeFull Start, DateTimeFull End)? Dates;
    public readonly string GoogleEventId;
    public readonly Uri? GoogleEvent;
    public readonly bool IsCancelled;
    public readonly bool IsDeleted;

    public PageInfo(Page page, Clock clock)
    {
        Page = page;
        _clock = clock;
        Title = GetTitle(page);
        Dates = GetDates(page);
        GoogleEventId = GetGoogleEventId(page);
        GoogleEvent = GetGoogleEvent(page);
        IsCancelled = GetStatus(page) == "Отменена";
        IsDeleted = Page.IsArchived;
    }

    private static string GetTitle(Page page)
    {
        if (page.Properties["Задача"] is not TitlePropertyValue title)
        {
            throw new NullReferenceException("\"Задача\" does not contain TitlePropertyValue.");
        }

        return JoinRichTextPart(title.Title);
    }

    private (DateTimeFull, DateTimeFull)? GetDates(Page page)
    {
        if (page.Properties["Дата"] is not DatePropertyValue date)
        {
            throw new NullReferenceException("\"Дата\" does not contain DatePropertyValue.");
        }

        return date.Date.Start is null || date.Date.End is null
            ? null
            : (_clock.GetDateTimeFull(date.Date.Start.Value.ToUniversalTime()),
                _clock.GetDateTimeFull(date.Date.End.Value.ToUniversalTime()));
    }

    private static string GetGoogleEventId(Page page)
    {
        if (page.Properties["Google Event Id"] is not RichTextPropertyValue eventId)
        {
            throw new NullReferenceException("\"Google Event Id\" does not contain RichTextPropertyValue.");
        }

        return JoinRichTextPart(eventId.RichText);
    }

    private static Uri? GetGoogleEvent(Page page)
    {
        if (page.Properties["Google Event"] is not UrlPropertyValue eventUrl)
        {
            throw new NullReferenceException("\"Google Event\" does not contain UrlPropertyValue.");
        }

        return string.IsNullOrWhiteSpace(eventUrl.Url) ? null : new Uri(eventUrl.Url);
    }

    private static string JoinRichTextPart(IEnumerable<RichTextBase> parts)
    {
        return string.Join("", parts.Select(r => r.PlainText));
    }

    private static string? GetStatus(Page page)
    {
        if (page.Properties["Статус"] is not StatusPropertyValue status)
        {
            throw new NullReferenceException("\"Статус\" does not contain StatusPropertyValue.");
        }

        return status.Status.Name;
    }

    private readonly Clock _clock;
}