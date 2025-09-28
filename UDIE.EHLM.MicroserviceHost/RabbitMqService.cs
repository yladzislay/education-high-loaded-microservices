using RabbitMQ.Client;
using UDIE.EHLM.Core;
using System;

namespace UDIE.EHLM.MicroserviceHost
{
    public class RabbitMqService : IDisposable
    {
        private IConnection? _connection;
        private IModel? _channel;
        private readonly MicroserviceInfo _serviceInfo;

        public RabbitMqService(MicroserviceInfo serviceInfo)
        {
            _serviceInfo = serviceInfo;
        }

        public void Initialize()
        {
            // We don't have the full queue configuration here yet, so we just try to connect
            // and declare the queues this service is interested in.
            if (_serviceInfo.IncomingQueues.Count == 0)
            {
                Console.WriteLine($"[{_serviceInfo.Name}] No incoming queues to declare.");
                return;
            }

            try
            {
                var factory = new ConnectionFactory() { HostName = "localhost" }; // Assumes RabbitMQ is running locally
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                Console.WriteLine($"[{_serviceInfo.Name}] Successfully connected to RabbitMQ.");

                foreach (var queueName in _serviceInfo.IncomingQueues)
                {
                    // For now, we declare a simple, durable queue.
                    // The logic for exchanges will be added in the next stages.
                    _channel.QueueDeclare(queue: queueName,
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    Console.WriteLine($"[{_serviceInfo.Name}] Declared queue: '{queueName}'");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{_serviceInfo.Name}] Could not initialize RabbitMQ: {ex.Message}");
                Console.ResetColor();
                // We allow the service to start even if RabbitMQ is unavailable.
            }
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            GC.SuppressFinalize(this);
        }
    }
}