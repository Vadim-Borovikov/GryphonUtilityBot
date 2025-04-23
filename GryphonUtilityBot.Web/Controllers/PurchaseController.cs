using System.Collections.Generic;
using GryphonUtilityBot.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using GryphonUtilityBot.Money;

namespace GryphonUtilityBot.Web.Controllers;

[Route("[controller]")]
public class PurchaseController : ControllerBase
{
    public PurchaseController(Config config) => _config = config;

    [HttpPost]
    public async Task<ActionResult> Post([FromServices] BotSingleton singleton, [FromBody] Purchase model)
    {
        foreach (Item item in model.Items)
        {
            Configs.Agent primary = _config.Texts.Agents[_config.PrimaryAgent];
            Configs.Agent secondary = _config.Texts.Agents[_config.SecondaryAgent];
            List<Transaction> transactions = item.GetTransactions(_config.PrimaryAgent, secondary.To, primary.To,
                _config.PurchaseCurrency).ToList();
            string note = string.Format(_config.ProductSoldNoteFormat, model.ClientName, item.Name);
            await singleton.Bot.AddSimultaneousTransactionsAsync(transactions, model.Date, note);
        }

        return Ok();
    }

    private readonly Config _config;
}