namespace UDIE.EHLM.Core
{
    public class StartupConfig
    {
        public MicroserviceInfo ServiceInfo { get; set; } = new();
        public Dictionary<string, RabbitMqQueueConfig> RabbitMQConfig { get; set; } = new();
    }
}