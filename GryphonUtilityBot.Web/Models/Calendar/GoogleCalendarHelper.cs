using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using GryphonUtilities;

namespace GryphonUtilityBot.Web.Models.Calendar;

internal sealed class GoogleCalendarHelper : IDisposable
{
    public GoogleCalendarHelper(Config config)
    {
        string json = string.IsNullOrWhiteSpace(config.GoogleCredentialJson)
            ? JsonSerializer.Serialize(config.GoogleCredential)
            : config.GoogleCredentialJson;
        BaseClientService.Initializer initializer = CreateInitializer(json, config.ApplicationName);
        _service = new CalendarService(initializer);
        _calendarId = config.GoogleCalendarId;
        _colorId = config.GoogleCalendarColorId;
    }

    public void Dispose() => _service.Dispose();

    public Task<Event> CreateEventAsync(string summary, DateTimeFull start, DateTimeFull end, string description)
    {
        Event body = new()
        {
            Summary = summary,
            Start = new EventDateTime { DateTime = start.ToDateTimeOffset().UtcDateTime },
            End = new EventDateTime { DateTime = end.ToDateTimeOffset().UtcDateTime },
            Description = description,
            ColorId = _colorId
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
        string description)
    {
        body.Summary = summary;
        body.Start = new EventDateTime { DateTime = start.ToDateTimeOffset().UtcDateTime };
        body.End = new EventDateTime { DateTime = end.ToDateTimeOffset().UtcDateTime };
        body.Description = description;
        EventsResource.UpdateRequest request = _service.Events.Update(body, _calendarId, id);
        return request.ExecuteAsync();
    }

    public Task DeleteEventAsync(Event calendarEvent)
    {
        if (IsDeleted(calendarEvent))
        {
            return Task.CompletedTask;
        }
        EventsResource.DeleteRequest request = _service.Events.Delete(_calendarId, calendarEvent.Id);
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