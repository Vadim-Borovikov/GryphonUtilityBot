using System.Collections.Generic;
using GoogleSheetsManager;

namespace GryphonUtilityBot.Vinland
{
    internal sealed class Activity : ILoadable
    {
        public string Name { get; private set; }
        public string Ability { get; private set; }

        public void Load(IDictionary<string, object> valueSet)
        {
            Name = valueSet[NameTitle]?.ToString();
            Ability = valueSet[AbilityTitle]?.ToString();
            _functional = valueSet[FunctionalTitle]?.ToBool() ?? false;
            _morning = valueSet[MorningTitle]?.ToBool() ?? false;
            _afternoon = valueSet[AfternoonTitle]?.ToBool() ?? false;
        }

        public bool IsRelevant(bool morning) => _functional && (morning ? _morning : _afternoon);

        private bool _functional;
        private bool _morning;
        private bool _afternoon;

        private const string NameTitle = "Название";
        private const string AbilityTitle = "Способность";
        private const string FunctionalTitle = "Работает";
        private const string MorningTitle = "Утро";
        private const string AfternoonTitle = "Вечер";
    }
}
