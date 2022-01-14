namespace GryphonUtilityBot.Vinland
{
    internal sealed class AbilityScores
    {
        public readonly bool XpBonus;
        public readonly bool XpPenalty;
        public readonly byte Value;

        public AbilityScores(bool xpBonus, bool xpPenalty, byte value)
        {
            XpBonus = xpBonus;
            XpPenalty = xpPenalty;
            Value = value;
        }
    }
}
