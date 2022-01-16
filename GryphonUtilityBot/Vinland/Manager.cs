﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AbstractBot;
using GoogleSheetsManager;
using GoogleSheetsManager.Providers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Vinland
{
    internal sealed class Manager
    {
        public Manager(Bot.Bot bot) => _bot = bot;

        public async Task RecommendAsync(ChatId chatId)
        {
            Message statusMessage =
                await _bot.Client.SendTextMessageAsync(chatId, "_Загружаю игровые данные…_", ParseMode.MarkdownV2);

            await LoadAsync();

            await _bot.Client.FinalizeStatusMessageAsync(statusMessage);

            IEnumerable<Option> options = FillOptions();
            Option best = options.OrderByDescending(o => o.GetScore()).First();
            await _bot.Client.SendTextMessageAsync(chatId, ShowOption(best), ParseMode.MarkdownV2);
        }

        private async Task LoadAsync()
        {
            IList<Ability> abilities;

            using (var provider =
                new SheetsProvider(_bot.Config.GoogleCredentialJson, ApplicationName, _bot.Config.GoogleSheetIdVinland))
            {
                _characters = await DataManager.GetValuesAsync<Character>(provider, _bot.Config.GoogleVinlandCharactersRange);
                _activities = await DataManager.GetValuesAsync<Activity>(provider, _bot.Config.GoogleVinlandActivitiesRange);
                abilities =
                    await DataManager.GetValuesAsync<Ability>(provider, _bot.Config.GoogleVinlandAbilitiesRange);
            }

            _characters = _characters.Where(c => c.Relevant).ToList();
            _activities = _activities.Where(a => a.Relevant).ToList();

            foreach (Ability ability in abilities)
            {
                Character character = _characters.SingleOrDefault(c => c.Name == ability.Character);
                if (character == null)
                {
                    continue;
                }

                Activity activity = _activities.FirstOrDefault(a => a.Ability == ability.Name);
                if (activity == null)
                {
                    continue;
                }

                character.Abilities[activity.Ability] = ability.Scores;
            }
        }

        private string ShowOption(Option option)
        {
            var sb = new StringBuilder();

            foreach (Activity activity in _activities.Where(a => option.Distribution.ContainsKey(a)))
            {
                sb.AppendLine($"{activity.Name}: {option.Distribution[activity].Name}");
            }

            List<Character> resting = _characters.Where(c => c.Relevant && !option.Distribution.ContainsValue(c)).ToList();
            if (resting.Any())
            {
                sb.AppendLine();
                sb.AppendLine($"Убежище: {string.Join(", ", resting.Select(c => c.Name))}");
            }

            return sb.ToString();
        }

        private IEnumerable<Option> FillOptions()
        {
            return _characters.Aggregate(new List<Option>(),
                (options, character) => AddCharacterToOptions(options, character).ToList());
        }

        private IEnumerable<Option> AddCharacterToOptions(ICollection<Option> options, Character character)
        {
            if (options.Count == 0)
            {
                foreach (Activity activity in _activities)
                {
                    yield return new Option(activity, character);
                }
                yield break;
            }

            if (options.First().Distribution.Count < _activities.Count)
            {
                foreach (Activity activity in _activities)
                {
                    foreach (Option option in options.Where(o => !o.Distribution.ContainsKey(activity))
                                                     .Select(o => o.Clone()))
                    {
                        option.Distribution[activity] = character;
                        yield return option;
                    }
                }
                yield break;
            }

            foreach (Option option in options)
            {
                yield return option;
            }

            foreach (Activity activity in _activities)
            {
                foreach (Option option in options.Select(o => o.Clone()))
                {
                    option.Distribution[activity] = character;
                    yield return option;
                }
            }
        }

        private const string ApplicationName = "GryphonUtilityBot";

        private readonly Bot.Bot _bot;
        private IList<Character> _characters;
        private IList<Activity> _activities;
    }
}
