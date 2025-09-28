using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;

namespace UDIE.EHLM.MicroserviceHost
{
    public class MicroservicesSimulator
    {
        public static async Task Run()
        {
            Console.WriteLine("Starting Microservices Simulator...");
            // No need to wait here, as it's run after services are up

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            var command = "RegisterUser";
            var body = Encoding.UTF8.GetBytes(command);

            // In the new design, we publish to the exchange, not a direct queue
            channel.BasicPublish(exchange: "auth_commands",
                                 routingKey: "auth_commands", // For direct exchange, routing key is the queue name
                                 basicProperties: null,
                                 body: body);

            Console.WriteLine($"[Simulator] Sent command '{command}' to exchange 'auth_commands'");
        }
    }
}