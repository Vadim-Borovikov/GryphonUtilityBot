using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GryphonUtilityBot.Web.Models
{
    public sealed class Config : Bot.Config
    {
        [JsonProperty]
        public string CultureInfoName { get; set; }

        [JsonProperty]
        public string AdminIdsJson { get; set; }

        [JsonProperty]
        public int PingPeriodSeconds { get; set; }

        [JsonProperty]
        public string PingUrisJson { get; set; }

        [JsonProperty]
        public List<Uri> PingUris { get; set; }

        internal TimeSpan PingPeriod => TimeSpan.FromSeconds(PingPeriodSeconds);
    }
}
