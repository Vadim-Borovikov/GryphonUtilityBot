using System;
using Newtonsoft.Json;

namespace GryphonUtilityBot.Web.Models.Config
{
    public sealed class Item
    {
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Half1 { get; set; }
        [JsonProperty]
        public string Half2 { get; set; }
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

        internal int GetRefillingAmount(int stocked, int days)
        {
            int needed = days * DailyNeed;
            int refillItems = Math.Max(needed - stocked, 0);
            return (int) Math.Ceiling(1.0 * refillItems / (PackSize * MetaPackSize));
        }

        internal bool HasHalves => !string.IsNullOrWhiteSpace(Half1) && !string.IsNullOrWhiteSpace(Half2);
    }
}
