using System.Collections.Generic;
using GoogleSheetsManager;

namespace GryphonUtilityBot.Vinland
{
    internal sealed class Activity : ILoadable
    {
        public string Name { get; private set; }
        public string Ability { get; private set; }
        public bool Relevant { get; private set; }

        public void Load(IDictionary<string, object> valueSet)
        {
            Name = valueSet[NameTitle]?.ToString();
            Ability = valueSet[AbilityTitle]?.ToString();
            Relevant = valueSet[RelevantTitle]?.ToBool() ?? false;
        }

        private const string NameTitle = "Название";
        private const string AbilityTitle = "Способность";
        private const string RelevantTitle = "Актуально";
    }
}
