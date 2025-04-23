using GryphonUtilityBot.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IO;
using System.Linq;
using System.Text.Json;
using GryphonUtilities;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Threading.Tasks;
using GryphonUtilityBot.Web.Models.Calendar.Notion;

namespace GryphonUtilityBot.Web.Controllers;

[Route("webhook/notion")]
public sealed class NotionWebhookController : Controller
{
    public NotionWebhookController(IOptions<Config> options, Logger logger)
    {
        _logger = logger;
        _secret = options.Value.NotionWebhookSecret;
    }

    public async Task<IActionResult> Post()
    {
        using (StreamReader reader = new(Request.Body))
        {
            string rawBody = await reader.ReadToEndAsync();

            JsonElement json = JsonSerializer.Deserialize<JsonElement>(rawBody);
            return json.TryGetProperty(VerificationTokenProperty, out JsonElement tokenJson)
                ? HandleVerificationUpdate(tokenJson)
                : HandleContentUpdate(rawBody);
        }
    }

    private OkResult HandleVerificationUpdate(JsonElement tokenJson)
    {
        string? token = tokenJson.GetString();
        _logger.LogTimedMessage($"Notion webhook verification token: {token}");
        return Ok();
    }

    private IActionResult HandleContentUpdate(string rawBody)
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
    private readonly Logger _logger;

    private const string VerificationTokenProperty = "verification_token";
    private const string SignatureHeader = "X-Notion-Signature";
    private const string SignaturePrefix = "sha256=";
}