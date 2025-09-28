using Newtonsoft.Json;

namespace UDIE.EHLM.Core
{
    public class RabbitMqQueueConfig
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "direct";
    }
}