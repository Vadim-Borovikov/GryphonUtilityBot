using GryphonUtilities;
using GryphonUtilities.Logging;
using GryphonUtilities.Time;
using GryphonUtilityBot.Web.Models;
using GryphonUtilityBot.Web.Models.Calendar;
using GryphonUtilityBot.Web.Models.Calendar.Notion;
using GryphonUtilityBot.Web.Models.Calendar.Notion.Updates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace GryphonUtilityBot.Web.Controllers;

[Route("webhook/notion")]
public sealed class NotionWebhookController : Controller
{
    public NotionWebhookController(Config config, Logger logger, IUpdatesSubscriber subscriber)
    {
        _logger = logger;
        _subscriber = subscriber;
        _secret = config.NotionWebhookSecret;
        _relevantParent = config.NotionDatabaseId;

        TimeSpan cleanupInterval = TimeSpan.FromHours(config.CleanupIntervalHours);
        _processedWebhookTtl = TimeSpan.FromHours(config.ProcessedWebhookTtlHours);

        UnboundedChannelOptions options = new()
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        };
        _updates = Channel.CreateUnbounded<Update>(options);

        Invoker.FireAndForget(_ => ProcessQueueAsync(), _logger);
        Invoker.DoPeriodically(_ => CleanupQueueAsync(), cleanupInterval, false, _logger, CancellationToken.None);
    }

    public async Task<IActionResult> Post()
    {
        using (StreamReader reader = new(Request.Body))
        {
            string rawBody = await reader.ReadToEndAsync();

            JsonElement json;
            try
            {
                json = JsonSerializer.Deserialize<JsonElement>(rawBody);
            }
            catch (JsonException ex)
            {
                _logger.Errors.Log(ex);
                return BadRequest();
            }

            return json.TryGetProperty(VerificationTokenProperty, out JsonElement tokenJson)
                ? HandleVerificationUpdate(tokenJson)
                : HandleContentUpdate(rawBody);
        }
    }

    private OkResult HandleVerificationUpdate(JsonElement tokenJson)
    {
        string? token = tokenJson.GetString();
        _logger.Messages.Log($"Notion webhook verification token: {token}", true);
        return Ok();
    }

    private IActionResult HandleContentUpdate(string rawBody)
    {
        if (!VerifySignature(rawBody))
        {
            _logger.Errors.Log("Signature verification failed.", true);
            return Unauthorized();
        }

        WebhookEvent? webhookEvent = TryParseEvent(rawBody);
        if (webhookEvent is null)
        {
            _logger.Errors.Log($"Failed to parse Notion webhook payload.{Environment.NewLine}{rawBody}", true);
            return BadRequest();
        }

        _logger.Messages.Log($"Succesfully parsed webhook payload.{Environment.NewLine}{rawBody}", false);

        if (!webhookEvent.Data.Parent.Id.Equals(_relevantParent, StringComparison.OrdinalIgnoreCase))
        {
            return NoContent();
        }

        Update update;
        switch (webhookEvent.Type)
        {
            case WebhookEvent.EventType.Created:
                update = new SimpleUpdate(webhookEvent.Id, webhookEvent.Entity.Id, SimpleUpdate.Type.Created);
                break;
            case WebhookEvent.EventType.PropertiesUpdated:
                if (webhookEvent.Data.UpdatedProperties is null)
                {
                    _logger.Errors.Log("Updated properties are null.", true);
                    return BadRequest();
                }
                update = new PropertiesUpdatedUpdate(webhookEvent.Id, webhookEvent.Entity.Id,
                    webhookEvent.Data.UpdatedProperties);
                break;
            case WebhookEvent.EventType.Moved:
                update = new MovedUpdate(webhookEvent.Id, webhookEvent.Entity.Id, webhookEvent.Data.Parent.Id);
                break;
            case WebhookEvent.EventType.Deleted:
                update = new SimpleUpdate(webhookEvent.Id, webhookEvent.Entity.Id, SimpleUpdate.Type.Deleted);
                break;
            case WebhookEvent.EventType.Undeleted:
                update = new SimpleUpdate(webhookEvent.Id, webhookEvent.Entity.Id, SimpleUpdate.Type.Undeleted);
                break;
            default:
                _logger.Messages.Log($"Unsupported Notion webhook event type: {webhookEvent.Type}.", false);
                return NoContent();
        }

        WebhookState state = new(WebhookState.WebhookStatus.Queued, DateTimeFull.CreateUtcNow());
        if (_webhookStates.TryAdd(webhookEvent.Id, state))
        {
            if (_updates.Writer.TryWrite(update))
            {
                return NoContent();
            }

            _webhookStates.Remove(webhookEvent.Id, out _);
            _logger.Errors.Log("Failed to write webhook event to channel.", true);
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        if (webhookEvent.AttemptNumber > 1)
        {
            _logger.Errors.Log($"Webhook event came again! Attempt number: {webhookEvent.AttemptNumber}.{Environment.NewLine}{rawBody}", true);
        }
        return NoContent();
    }

    private bool VerifySignature(string rawBody)
    {
        if (string.IsNullOrWhiteSpace(_secret))
        {
            return false;
        }

        string? signature = Request.Headers[SignatureHeader].SingleOrDefault();
        if (string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        if (signature.StartsWith(SignaturePrefix, StringComparison.OrdinalIgnoreCase))
        {
            signature = signature.Substring(SignaturePrefix.Length);
        }

        using (HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(_secret)))
        {
            byte[] bodyBytes = Encoding.UTF8.GetBytes(rawBody);
            byte[] hashBytes = hmac.ComputeHash(bodyBytes);
            string hash = Convert.ToHexString(hashBytes);
            return string.Equals(hash, signature, StringComparison.OrdinalIgnoreCase);
        }
    }

    private WebhookEvent? TryParseEvent(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<WebhookEvent>(json, Config.JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.Errors.Log(ex);
            return null;
        }
    }

    private async Task ProcessQueueAsync()
    {
        await foreach (Update update in _updates.Reader.ReadAllAsync())
        {
            _webhookStates[update.WebhookId] =
                new WebhookState(WebhookState.WebhookStatus.Processing, DateTimeFull.CreateUtcNow());

            try
            {
                await _subscriber.ProcessAsync(update);
                _webhookStates[update.WebhookId] =
                    new WebhookState(WebhookState.WebhookStatus.Processed, DateTimeFull.CreateUtcNow());
            }
            catch (Exception ex)
            {
                _logger.Errors.Log(ex);
                _webhookStates.Remove(update.WebhookId, out _);
            }
        }
    }

    private Task CleanupQueueAsync()
    {
        DateTimeFull threshold = DateTimeFull.CreateUtcNow() - _processedWebhookTtl;
        List<string> toDelete = _webhookStates.Where(p => (p.Value.Status == WebhookState.WebhookStatus.Processed)
                                                          && (p.Value.UpdatedAt <= threshold))
                                              .Select(p => p.Key)
                                              .ToList();
        foreach (string id in toDelete)
        {
            _webhookStates.Remove(id, out _);
        }
        return Task.CompletedTask;
    }

    private readonly string? _secret;
    private readonly string _relevantParent;
    private readonly Logger _logger;
    private readonly IUpdatesSubscriber _subscriber;
    private readonly Channel<Update> _updates;
    private readonly ConcurrentDictionary<string, WebhookState> _webhookStates = new();
    private readonly TimeSpan _processedWebhookTtl;

    private const string VerificationTokenProperty = "verification_token";
    private const string SignatureHeader = "X-Notion-Signature";
    private const string SignaturePrefix = "sha256=";
}