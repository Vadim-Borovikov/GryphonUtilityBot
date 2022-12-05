using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GryphonUtilityBot;

internal sealed class InsuranceManager
{
    internal bool Active { get; private set; }

    public InsuranceManager(Bot bot)
    {
        _bot = bot;
        KeyboardButton home = new(HomeCaption);
        _homeKeyboard = new ReplyKeyboardMarkup(home);
    }

    public void Reset()
    {
        _address = null;
        _problem = null;
        Active = false;
    }

    public Task StartDiscussion(Chat chat)
    {
        _address = null;
        _problem = null;
        Active = true;
        return AskForAddress(chat);
    }

    public Task Accept(Chat chat, Message message)
    {
        if (_address is null)
        {
            return AcceptAddress(chat, message.Text);
        }

        if (_problem is null)
        {
            return AcceptProblem(chat, message.Text);
        }

        return Task.CompletedTask;
    }

    private Task AskForAddress(Chat chat) => _bot.SendTextMessageAsync(chat, "Где ты сейчас?", _homeKeyboard);

    private Task AskForProblem(Chat chat) => _bot.SendTextMessageAsync(chat, "Что случилось?");

    private async Task AcceptAddress(Chat chat, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            await _bot.SendTextMessageAsync(chat, "Адрес пуст.");
            await AskForAddress(chat);
            return;
        }

        _address = text == HomeCaption ? _bot.Config.DefaultAddress : text;
        await AskForProblem(chat);
    }

    private async Task AcceptProblem(Chat chat, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            await _bot.SendTextMessageAsync(chat, "Описание случившегося пусто.");
            await AskForProblem(chat);
            return;
        }

        _problem = AbstractBot.Utils.EscapeCharacters(text);
        await GenerateAndSendMessage(chat);
        Reset();
    }

    private Task GenerateAndSendMessage(Chat chat)
    {
        TimeSpan timeSpan =
            _bot.TimeManager.Now() - _bot.TimeManager.GetDateTimeFull(_bot.Config.ArrivalDate, TimeOnly.MinValue);
        uint days = (uint) timeSpan.TotalDays;
        string messsage = string.Format(_bot.Config.InsuranceMessageFormat, _address,
            _bot.Config.ArrivalDate.ToString("dd MMMM yyyy"), days, _problem);
        return _bot.SendTextMessageAsync(chat, messsage, ParseMode.MarkdownV2);
    }

    private const string HomeCaption = "Дома";

    private readonly Bot _bot;
    private readonly IReplyMarkup _homeKeyboard;

    private string? _address;
    private string? _problem;
}