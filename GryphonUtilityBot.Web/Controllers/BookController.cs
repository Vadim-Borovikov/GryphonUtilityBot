using System;
using GryphonUtilityBot.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GryphonUtilities.Extensions;
using Microsoft.Extensions.Options;

namespace GryphonUtilityBot.Web.Controllers;

[Route("[controller]")]
public class BookController : ControllerBase
{
    public BookController(IOptions<Config> config) => _config = config.Value;

    [HttpPost]
    public async Task<ActionResult> Post([FromServices] BotSingleton singleton, [FromForm] Submission model,
        [FromForm] IFormCollection form)
    {
        if (model.Test == TestString)
        {
            return Ok();
        }

        if ((model.FormId != _config.TildaFormId) || string.IsNullOrWhiteSpace(model.Name) || model.Email is null
            || string.IsNullOrWhiteSpace(model.Telegram) || string.IsNullOrWhiteSpace(model.Items))
        {
            return BadRequest(ModelState);
        }

        List<string> items = model.Items.Split(ItemsSeparator).Select(s => s.Trim()).ToList();

        List<Uri> slips = form.Where(p => p.Key.StartsWith(FilePrefix, StringComparison.OrdinalIgnoreCase))
                              .SelectMany(p => p.Value)
                              .RemoveNulls()
                              .Select(s => new Uri(s))
                              .ToList();
        if (slips.Count == 0)
        {
            return BadRequest(ModelState);
        }

        await singleton.Bot.OnSubmissionReceivedAsync(model.Name, model.Email, model.Telegram, items, slips);

        return Ok();
    }

    private const string TestString = "test";
    private const string FilePrefix = "file";
    private const char ItemsSeparator = ';';

    private readonly Config _config;
}