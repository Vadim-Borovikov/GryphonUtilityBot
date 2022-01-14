using System.Collections.Generic;
using GoogleSheetsManager;

namespace GryphonUtilityBot.Vinland
{
    internal sealed class Connection : ILoadable
    {
        public string Character { get; private set; }
        public string Ability { get; private set; }
        public AbilityScores Scores { get; private set; }

        public void Load(IDictionary<string, object> valueSet)
        {
            Character = valueSet[CharacterTitle]?.ToString();
            Ability = valueSet[AbilityTitle]?.ToString();

            bool xpBonus = valueSet[XpBonusTitle]?.ToBool() ?? false;
            bool xpPenalty = valueSet[XpPenaltyTitle]?.ToBool() ?? false;
            byte value = valueSet[ValueTitle]?.ToByte() ?? 0;
            Scores = new AbilityScores(xpBonus, xpPenalty, value);
        }

        private const string CharacterTitle = "Персонаж";
        private const string AbilityTitle = "Способность";
        private const string XpBonusTitle = "Бонус к XP";
        private const string XpPenaltyTitle = "Штраф к XP";
        private const string ValueTitle = "Значение";
    }
}
