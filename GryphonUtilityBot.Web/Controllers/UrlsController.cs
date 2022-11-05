using System;
using System.Collections.Generic;
using GryphonUtilityBot.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace GryphonUtilityBot.Web.Controllers;

[Route("urls")]
public sealed class UrlsController : Controller
{
    public List<Uri> Get([FromServices] BotSingleton singleton) => singleton.PingUrls;
}