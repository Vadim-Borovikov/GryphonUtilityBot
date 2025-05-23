﻿using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using GryphonUtilities.Time;

namespace GryphonUtilityBot.Web.Models.Calendar;

internal sealed class GoogleCalendarProvider : IDisposable
{
    public GoogleCalendarProvider(Config config)
    {
        string json = JsonSerializer.Serialize(config.GoogleCredential);
        BaseClientService.Initializer initializer = CreateInitializer(json, config.ApplicationName);
        _service = new CalendarService(initializer);
        _calendarId = config.GoogleCalendarId;
        _colorId = config.GoogleCalendarColorId;
    }

    public void Dispose() => _service.Dispose();

    public Task<Event> CreateEventAsync(string summary, DateTimeFull start, DateTimeFull end, string description,
        string? location)
    {
        Event body = new()
        {
            Summary = summary,
            Start = new EventDateTime { DateTimeDateTimeOffset = start.UtcDateTime },
            End = new EventDateTime { DateTimeDateTimeOffset = end.UtcDateTime },
            Description = description,
            ColorId = _colorId,
            Location = location
        };
        EventsResource.InsertRequest request = _service.Events.Insert(body, _calendarId);
        return request.ExecuteAsync();
    }

    public async Task<Event?> GetEventAsync(string id)
    {
        try
        {
            EventsResource.GetRequest request = _service.Events.Get(_calendarId, id);
            Event result = await request.ExecuteAsync();
            return IsDeleted(result) ? null : result;
        }
        catch (GoogleApiException e) when (e.HttpStatusCode is HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public Task UpdateEventAsync(string id, Event body, string summary, DateTimeFull start, DateTimeFull end,
        string description, string? location)
    {
        body.Summary = summary;
        body.Start = new EventDateTime { DateTimeDateTimeOffset = start.UtcDateTime };
        body.End = new EventDateTime { DateTimeDateTimeOffset = end.UtcDateTime };
        body.Description = description;
        body.Location = location;
        EventsResource.UpdateRequest request = _service.Events.Update(body, _calendarId, id);
        return request.ExecuteAsync();
    }

    public Task DeleteEventAsync(string id)
    {
        EventsResource.DeleteRequest request = _service.Events.Delete(_calendarId, id);
        return request.ExecuteAsync();
    }

    private static bool IsDeleted(Event calendarEvent) => calendarEvent.Status == "cancelled";

    private static BaseClientService.Initializer CreateInitializer(string credentialJson, string applicationName)
    {
        GoogleCredential credential = GoogleCredential.FromJson(credentialJson).CreateScoped(Scopes);
        return new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = applicationName
        };
    }

    private static readonly string[] Scopes = { CalendarService.Scope.CalendarEvents };

    private readonly CalendarService _service;
    private readonly string _calendarId;
    private readonly string _colorId;
}