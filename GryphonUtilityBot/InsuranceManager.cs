using System;
using System.Threading.Tasks;
using GryphonUtilities;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GryphonUtilityBot;

internal sealed class InsuranceManager
{
    internal bool Active { get; private set; }

    public InsuranceManager(Bot bot, string messageFormat, string defaultAddress, DateOnly arrivalDate)
    {
        _bot = bot;
        _messageFormat = messageFormat;
        _defaultAddress = defaultAddress;
        _arrivalDate = arrivalDate;
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

        _address = text == HomeCaption ? _defaultAddress : text;
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
        TimeSpan timeSpan = DateTimeFull.CreateUtcNow() - DateTimeFull.CreateUtc(_arrivalDate, TimeOnly.MinValue);
        uint days = (uint) timeSpan.TotalDays;
        string messsage =
            string.Format(_messageFormat, _address, _arrivalDate.ToString("dd MMMM yyyy"), days, _problem);
        return _bot.SendTextMessageAsync(chat, messsage, ParseMode.MarkdownV2);
    }

    private const string HomeCaption = "Дома";

    private readonly Bot _bot;
    private readonly string _messageFormat;
    private readonly string _defaultAddress;
    private readonly DateOnly _arrivalDate;
    private readonly IReplyMarkup _homeKeyboard;

    private string? _address;
    private string? _problem;
}