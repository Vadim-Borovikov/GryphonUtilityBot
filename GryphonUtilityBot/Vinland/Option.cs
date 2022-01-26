using System.Collections.Generic;

namespace GryphonUtilityBot.Vinland
{
    internal sealed class Option
    {
        public readonly Dictionary<Activity, Character> Distribution;

        public Option(Activity activity, Character character, short activityPriorityScore)
            : this(new Dictionary<Activity, Character> { [activity] = character }, activityPriorityScore)
        { }

        public decimal GetScore()
        {
            decimal score = 0;
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (Activity activity in Distribution.Keys)
            {
                Character character = Distribution[activity];
                decimal currentScore = character.Abilities[activity.Ability];
                if (activity.Priority)
                {
                    currentScore *= _activityPriorityScore;
                }
                score += currentScore;
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
            return new Option(distribution, _activityPriorityScore);
        }

        private Option(Dictionary<Activity, Character> distribution, short activityPriorityScore)
        {
            Distribution = distribution;
            _activityPriorityScore = activityPriorityScore;
        }

        private readonly short _activityPriorityScore;
    }
}
