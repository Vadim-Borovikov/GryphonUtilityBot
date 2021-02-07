using Newtonsoft.Json;

namespace GryphonUtilityBot.Web.Models
{
    public sealed class Config : Bot.Config
    {
        [JsonProperty]
        public string CultureInfoName { get; set; }

        [JsonProperty]
        public string GoogleCredentialJson { get; set; }

        [JsonProperty]
        public string AdminIdsJson { get; set; }
    }
}
