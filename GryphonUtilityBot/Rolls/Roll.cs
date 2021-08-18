using System;
using System.Collections.Generic;
using System.Linq;

namespace GryphonUtilityBot.Rolls
{
    internal sealed class Roll
    {
        public readonly List<byte> D6Slashing;
        public readonly byte BonusSlashing;
        public readonly List<byte> D6Fire;

        public readonly byte ResultSlashing;
        public readonly byte ResultFire;

        public Roll(byte d6Slashing, byte bonusSlashing, byte d6Fire)
        {
            D6Slashing = MakeD6Rolls(d6Slashing).ToList();
            BonusSlashing = bonusSlashing;
            D6Fire = MakeD6Rolls(d6Fire).ToList();

            ResultSlashing = (byte)(D6Slashing.Sum(x => x) + BonusSlashing);
            ResultFire = (byte)D6Fire.Sum(x => x);
        }

        private static IEnumerable<byte> MakeD6Rolls(byte amount)
        {
            for (byte i = 0; i < amount; ++i)
            {
                yield return MakeD6Roll();
            }
        }

        private static byte MakeD6Roll() => (byte)Random.Next(1, 7);

        private static readonly Random Random = new Random();
    }
}
