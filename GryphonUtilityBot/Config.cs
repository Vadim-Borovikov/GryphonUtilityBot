using Newtonsoft.Json;

namespace GryphonUtilityBot
{
    public class Config : AbstractBot.Config
    {
        [JsonProperty]
        public int MistressId { get; set; }
    }
}
