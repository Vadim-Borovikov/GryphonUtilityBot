using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GryphonUtilityBot.Rolls
{
    internal sealed class Manager
    {
        public Manager(Bot.Bot bot)
        {
            _bot = bot;

            _greatWeaponMasterOption =
                new InlineKeyboardButton(GreatWeaponMasterText) { CallbackData = GreatWeaponMasterText };
            _rageOption = new InlineKeyboardButton(RageText) { CallbackData = RageText };
            _flameOption = new InlineKeyboardButton(FlameText) { CallbackData = FlameText };
            _critOption = new InlineKeyboardButton(CritText) { CallbackData = CritText };

            var firstRaw = new[] { _rageOption, _greatWeaponMasterOption };
            var secondRaw = new[] { _flameOption, _critOption };

            _options = new InlineKeyboardMarkup(new[] { firstRaw, secondRaw });
        }

        public Task ProcessQueryAsync(string callbackQueryData, Message message)
        {
            switch (callbackQueryData)
            {
                case GreatWeaponMasterText:
                    _greatWeaponMaster = !_greatWeaponMaster;
                    break;
                case RageText:
                    _rage = !_rage;
                    break;
                case FlameText:
                    _flame = !_flame;
                    break;
                case CritText:
                    _crit = !_crit;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            UpdateValues();

            return _bot.Client.EditMessageTextAsync(message.Chat, message.MessageId, _prefix, replyMarkup: _options);
        }

        public async Task PrepareToRoll(ChatId chatId)
        {
            _greatWeaponMaster = false;
            _rage = false;
            _flame = false;
            _crit = false;

            UpdateValues();

            await _bot.Client.SendTextMessageAsync(chatId, _prefix, replyMarkup: _options);
        }

        public async Task Roll(ChatId chatId)
        {
            string prefix = GetMessage(_d6Slashing, _bonusSlashing, _d6Fire);

            var roll = new Roll(_d6Slashing, _bonusSlashing, _d6Fire);
            string message = GetMessage(prefix, roll);
            await _bot.Client.SendTextMessageAsync(chatId, message);

            await _bot.Client.SendTextMessageAsync(chatId, _prefix, replyMarkup: _options);
        }

        private static string GetMessage(byte d6Slashing, byte bonusSlashing, byte d6Fire)
        {
            string result = $"{d6Slashing}d6+{bonusSlashing}🔪";
            if (d6Fire > 0)
            {
                result += $"{d6Fire}d6🔥";
            }
            return result;
        }

        private static string GetMessage(string prefix, Roll roll)
        {
            bool fire = roll.D6Fire.Count > 0;
            var sb = new StringBuilder();

            sb.AppendLine($"{prefix} =");

            sb.Append($"{GetEmoji(roll.D6Slashing)}+{roll.BonusSlashing}🔪");
            if (fire)
            {
                sb.Append($"{GetEmoji(roll.D6Fire)}🔥");
            }
            sb.AppendLine(" =");

            sb.Append($"{roll.ResultSlashing}🔪");
            if (fire)
            {
                sb.Append($"{roll.ResultFire}🔥");
            }

            return sb.ToString();
        }

        private void UpdateValues()
        {
            _d6Slashing = 2;

            _bonusSlashing = 5;
            if (_greatWeaponMaster)
            {
                _bonusSlashing += 10;
            }

            if (_rage)
            {
                _bonusSlashing += 2;
            }

            _d6Fire = 0;
            if (_flame)
            {
                _d6Fire = 2;
            }

            if (_crit)
            {
                _d6Slashing *= 2;
                _d6Fire *= 2;
            }

            _greatWeaponMasterOption.Text = _greatWeaponMaster ? $"✅{GreatWeaponMasterText}" : $"{GreatWeaponMasterText}";
            _rageOption.Text = _rage ? $"✅{RageText}" : $"{RageText}";
            _flameOption.Text = _flame ? $"✅{FlameText}" : $"{FlameText}";
            _critOption.Text = _crit ? $"✅{CritText}" : $"{CritText}";

            _prefix = GetMessage(_d6Slashing, _bonusSlashing, _d6Fire);
        }

        private static string GetEmoji(IEnumerable<byte> numbers) => string.Join("", numbers.Select(GetEmoji));

        private static string GetEmoji(byte number)
        {
            switch (number)
            {
                case 1: return "1⃣";
                case 2: return "2⃣";
                case 3: return "3⃣";
                case 4: return "4⃣";
                case 5: return "5⃣";
                case 6: return "6⃣";
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private readonly Bot.Bot _bot;

        private byte _d6Slashing;
        private byte _bonusSlashing;
        private byte _d6Fire;

        private bool _greatWeaponMaster;
        private bool _rage;
        private bool _flame;
        private bool _crit;

        private const string GreatWeaponMasterText = "Размашистая";
        private const string RageText = "Яростная";
        private const string FlameText = "Огненная";
        private const string CritText = "Крит!";

        private readonly InlineKeyboardButton _greatWeaponMasterOption;
        private readonly InlineKeyboardButton _rageOption;
        private readonly InlineKeyboardButton _flameOption;
        private readonly InlineKeyboardButton _critOption;
        private readonly InlineKeyboardMarkup _options;

        private string _prefix;
    }
}
