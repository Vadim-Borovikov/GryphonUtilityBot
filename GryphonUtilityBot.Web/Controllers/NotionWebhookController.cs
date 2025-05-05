using GryphonUtilityBot.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Text.Json;
using GryphonUtilities;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Threading.Tasks;
using GryphonUtilityBot.Web.Models.Calendar;
using GryphonUtilityBot.Web.Models.Calendar.Notion;

namespace GryphonUtilityBot.Web.Controllers;

[Route("webhook/notion")]
public sealed class NotionWebhookController : Controller
{
    public NotionWebhookController(Config config, Logger logger, IUpdatesSubscriber subscriber)
    {
        _logger = logger;
        _subscriber = subscriber;
        _secret = config.NotionWebhookSecret;
        _relevatnParent = config.NotionDatabaseId;
    }

    public async Task<IActionResult> Post()
    {
        using (StreamReader reader = new(Request.Body))
        {
            string rawBody = await reader.ReadToEndAsync();

            JsonElement json = JsonSerializer.Deserialize<JsonElement>(rawBody);
            return json.TryGetProperty(VerificationTokenProperty, out JsonElement tokenJson)
                ? HandleVerificationUpdate(tokenJson)
                : await HandleContentUpdate(rawBody);
        }
    }

    private OkResult HandleVerificationUpdate(JsonElement tokenJson)
    {
        string? token = tokenJson.GetString();
        _logger.LogTimedMessage($"Notion webhook verification token: {token}");
        return Ok();
    }

    private async Task<IActionResult> HandleContentUpdate(string rawBody)
    {
        if (!VerifySignature(rawBody))
        {
            _logger.LogError("Signature verification failed.");
            return Unauthorized();
        }

        WebhookEvent? webhookEvent = TryParseEvent(rawBody);
        if (webhookEvent is null)
        {
            _logger.LogError($"Failed to parse Notion webhook payload.{Environment.NewLine}{rawBody}");
            return BadRequest();
        }

        _logger.LogTimedMessage($"Succesfully parsed webhook payload.{Environment.NewLine}{rawBody}");

        if (!webhookEvent.Data.Parent.Id.Equals(_relevatnParent, StringComparison.OrdinalIgnoreCase))
        {
            return Ok();
        }

        switch (webhookEvent.Type)
        {
            case WebhookEvent.EventType.Created:
                await _subscriber.OnCreatedAsync(webhookEvent.Entity.Id);
                break;
            case WebhookEvent.EventType.PropertiesUpdated:
                if (webhookEvent.Data.UpdatedProperties is null)
                {
                    _logger.LogError("Updated properties are null.");
                    return BadRequest();
                }
                await _subscriber.OnPropertiesUpdatedAsync(webhookEvent.Entity.Id, webhookEvent.Data.UpdatedProperties);
                break;
            case WebhookEvent.EventType.Moved:
                await _subscriber.OnMovedAsync(webhookEvent.Entity.Id, webhookEvent.Data.Parent.Id);
                break;
            case WebhookEvent.EventType.Deleted:
                await _subscriber.OnDeletedAsync(webhookEvent.Entity.Id);
                break;
            case WebhookEvent.EventType.Undeleted:
                await _subscriber.OnUndeletedAsync(webhookEvent.Entity.Id);
                break;
            default: throw new ArgumentOutOfRangeException();
        }

        return Ok();
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
            _logger.LogException(ex);
            return null;
        }
    }

    private readonly string? _secret;
    private readonly string _relevatnParent;
    private readonly Logger _logger;
    private readonly IUpdatesSubscriber _subscriber;

    private const string VerificationTokenProperty = "verification_token";
    private const string SignatureHeader = "X-Notion-Signature";
    private const string SignaturePrefix = "sha256=";
}