using System.Collections.Generic;
using GoogleSheetsManager;

namespace GryphonUtilityBot.Vinland
{
    internal sealed class Ability : ILoadable
    {
        public string Character { get; private set; }
        public string Name { get; private set; }
        public decimal Score { get; private set; }

        public void Load(IDictionary<string, object> valueSet)
        {
            Character = valueSet[CharacterTitle]?.ToString();
            Name = valueSet[NameTitle]?.ToString();

            byte value = valueSet[ValueTitle]?.ToByte() ?? 0;

            decimal xpModifier = valueSet[XpModifierTitle]?.ToDecimal() ?? 0;

            Score = value * xpModifier;
        }

        private const string CharacterTitle = "Персонаж";
        private const string NameTitle = "Способность";
        private const string ValueTitle = "Значение";
        private const string XpModifierTitle = "Модификатор XP";
    }
}
