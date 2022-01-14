using System.Collections.Generic;

namespace GryphonUtilityBot.Vinland
{
    internal sealed class Option
    {
        public readonly Dictionary<Activity, Character> Distribution;

        public Option(Activity activity, Character character)
            : this(new Dictionary<Activity, Character> { [activity] = character })
        { }

        public short GetScore()
        {
            short score = 0;
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (Activity activity in Distribution.Keys)
            {
                Character character = Distribution[activity];
                AbilityScores scores = character.Abilities[activity.Ability];
                if (scores.XpBonus)
                {
                    score += 1000;
                }
                if (scores.XpPenalty)
                {
                    score -= 1000;
                }
                score += scores.Value;
            }
            return score;
        }

        public Option Clone()
        {
            var distribution = new Dictionary<Activity, Character>();
            foreach (Activity activity in Distribution.Keys)
            {
                distribution[activity] = Distribution[activity];
            }
            return new Option(distribution);
        }

        private Option(Dictionary<Activity, Character> distribution) => Distribution = distribution;
    }
}
