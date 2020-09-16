﻿using System;
using Newtonsoft.Json;

namespace ShoppingHelper.Logic
{
    public sealed class Item
    {
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public int DailyNeed { get; set; }
        [JsonProperty]
        public int PackSize { get; set; }

        public int GetRefillingAmount(int stocked, int days)
        {
            int needed = days * DailyNeed;
            int refillItems = Math.Max(needed - stocked, 0);
            return (int) Math.Ceiling(1.0 * refillItems / PackSize);
        }
    }
}
