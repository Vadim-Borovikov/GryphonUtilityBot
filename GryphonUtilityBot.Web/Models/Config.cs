using Newtonsoft.Json;

namespace GryphonUtilityBot.Web.Models
{
    public sealed class Config : Bot.Config.Config
    {
        [JsonProperty]
        public string CultureInfoName { get; set; }

        [JsonProperty]
        public string GoogleCredentialsJson { get; set; }
    }
}
