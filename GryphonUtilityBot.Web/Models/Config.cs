using Newtonsoft.Json;

namespace GryphonUtilityBot.Web.Models
{
    public sealed class Config : GryphonUtilityBot.Config
    {
        [JsonProperty]
        public string CultureInfoName { get; set; }

        [JsonProperty]
        public string AdminIdsJson { get; set; }
    }
}
