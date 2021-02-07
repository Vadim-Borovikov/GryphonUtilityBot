using System;
using Newtonsoft.Json;

namespace GryphonUtilityBot.Bot.Config
{
    public sealed class ShopItem
    {
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Half1 { get; set; }
        [JsonProperty]
        public string Half2 { get; set; }
        [JsonProperty]
        public decimal Mass { get; set; }
        [JsonProperty]
        public int DailyNeed { get; set; }
        [JsonProperty]
        public int PackSize { get; set; }
        [JsonProperty]
        public int MetaPackSize { get; set; }
        [JsonProperty]
        public int AskOrder { get; set; }
        [JsonProperty]
        public int ResultOrder { get; set; }
        [JsonProperty]
        public Uri Uri { get; set; }
        [JsonProperty]
        public Uri UriHalf1 { get; set; }
        [JsonProperty]
        public Uri UriHalf2 { get; set; }

        internal int GetRefillingAmount(int stocked, int days)
        {
            int needed = days * DailyNeed;
            int refillItems = Math.Max(needed - stocked, 0);
            return (int) Math.Ceiling(1.0 * refillItems / (PackSize * MetaPackSize));
        }

        internal decimal GetRefillingMass(int amount)
        {
            decimal mass = Mass * amount;
            return Math.Ceiling(mass * 10) / 10;
        }

        internal bool HasHalves => !string.IsNullOrWhiteSpace(Half1) && !string.IsNullOrWhiteSpace(Half2);
        internal bool HasMass => Mass > 0;
    }
}
