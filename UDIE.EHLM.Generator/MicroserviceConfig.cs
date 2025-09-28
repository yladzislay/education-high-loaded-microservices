namespace UDIE.EHLM.Generator;

public class MicroserviceConfig
{
    public List<MicroserviceInfo> Microservices { get; set; }
    public Dictionary<string, Dictionary<string, object>> RabbitMQQueues { get; set; }
}