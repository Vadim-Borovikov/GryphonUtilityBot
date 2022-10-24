using System;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Newtonsoft.Json;

namespace GryphonUtilityBot.Web.Models.Calendar;

internal sealed class GoogleCalendarHelper : IDisposable
{
    public GoogleCalendarHelper(Config config)
    {
        string json = string.IsNullOrWhiteSpace(config.GoogleCredentialJson)
            ? JsonConvert.SerializeObject(config.GoogleCredential)
            : config.GoogleCredentialJson;
        BaseClientService.Initializer initializer = CreateInitializer(json, config.ApplicationName);
        _service = new CalendarService(initializer);
        _calendarId = config.GoogleCalendarId;
        _colorId = config.GoogleCalendarColorId;
    }

    public void Dispose() => _service.Dispose();

    public Task<Event> CreateEventAsync(string summary, DateTime start, DateTime end, string description)
    {
        Event body = new()
        {
            Summary = summary,
            Start = new EventDateTime { DateTime = start },
            End = new EventDateTime { DateTime = end },
            Description = description,
            ColorId = _colorId
        };
        EventsResource.InsertRequest request = _service.Events.Insert(body, _calendarId);
        return request.ExecuteAsync();
    }

    public Task<Event> GetEventAsync(string id)
    {
        EventsResource.GetRequest request = _service.Events.Get(_calendarId, id);
        return request.ExecuteAsync();
    }

    public Task UpdateEventAsync(string id, Event body, string summary, DateTime start, DateTime end,
        string description)
    {
        body.Summary = summary;
        body.Start = new EventDateTime { DateTime = start };
        body.End = new EventDateTime { DateTime = end };
        body.Description = description;
        EventsResource.UpdateRequest request = _service.Events.Update(body, _calendarId, id);
        return request.ExecuteAsync();
    }

    public Task DeleteEventAsync(string id)
    {
        EventsResource.DeleteRequest request = _service.Events.Delete(_calendarId, id);
        return request.ExecuteAsync();
    }

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