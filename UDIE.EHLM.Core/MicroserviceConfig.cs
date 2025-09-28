namespace UDIE.EHLM.Core;

public class MicroserviceConfig
{
    public List<MicroserviceInfo> Microservices { get; set; } = new();
    public Dictionary<string, Dictionary<string, object>> RabbitMQQueues { get; set; } = new();
}