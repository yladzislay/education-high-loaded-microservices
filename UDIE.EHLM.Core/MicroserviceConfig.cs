namespace UDIE.EHLM.Core;

using Newtonsoft.Json;

public class MicroserviceConfig
{
    public List<MicroserviceInfo> Microservices { get; set; } = new();

    [JsonProperty("RabbitMQQueues")]
    public Dictionary<string, RabbitMqQueueConfig> RabbitMQQueues { get; set; } = new();
}