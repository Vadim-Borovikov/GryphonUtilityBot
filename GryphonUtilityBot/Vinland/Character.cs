using System.Collections.Generic;
using GoogleSheetsManager;

namespace GryphonUtilityBot.Vinland
{
    internal sealed class Character : ILoadable
    {
        public string Name { get; private set; }
        public bool Relevant { get; private set; }

        public readonly Dictionary<string, decimal> Abilities = new Dictionary<string, decimal>();

        public void Load(IDictionary<string, object> valueSet)
        {
            Name = valueSet[NameTitle]?.ToString();
            Relevant = valueSet[RelevantTitle]?.ToBool() ?? false;
        }

        private const string NameTitle = "Имя";
        private const string RelevantTitle = "Актуален";
    }
}
