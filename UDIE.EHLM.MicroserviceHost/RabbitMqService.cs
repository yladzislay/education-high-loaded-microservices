using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UDIE.EHLM.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace UDIE.EHLM.MicroserviceHost
{
    public class RabbitMqService : IDisposable
    {
        private IConnection? _connection;
        private IModel? _channel;
        private readonly MicroserviceInfo _serviceInfo;
        private readonly Dictionary<string, RabbitMqQueueConfig> _rabbitMqConfig;

        public RabbitMqService(MicroserviceInfo serviceInfo, Dictionary<string, RabbitMqQueueConfig> rabbitMqConfig)
        {
            _serviceInfo = serviceInfo;
            _rabbitMqConfig = rabbitMqConfig;
        }

        public void Initialize()
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                Console.WriteLine($"[{_serviceInfo.Name}] Successfully connected to RabbitMQ.");

                SetupExchangesAndQueues();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{_serviceInfo.Name}] Could not initialize RabbitMQ: {ex.Message}");
                Console.ResetColor();
            }
        }

        private void SetupExchangesAndQueues()
        {
            if (_channel == null) return;

            Console.WriteLine($"[{_serviceInfo.Name}] Setting up exchanges and queues...");

            // Declare all exchanges defined in the config
            foreach (var queueConfig in _rabbitMqConfig)
            {
                var exchangeName = queueConfig.Key;
                var exchangeType = queueConfig.Value.Type;
                _channel.ExchangeDeclare(exchange: exchangeName, type: exchangeType, durable: true);
                Console.WriteLine($"[{_serviceInfo.Name}] Declared exchange '{exchangeName}' of type '{exchangeType}'.");
            }

            // For each incoming queue for this service, declare it and bind it to the correct exchange
            foreach (var queueName in _serviceInfo.IncomingQueues)
            {
                // The queue name is the same as the exchange/routing key in our design
                _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                // Bind the queue to the exchange. The routing key for fanout is empty.
                // For direct, the routing key is the queue name itself.
                var exchangeType = _rabbitMqConfig.GetValueOrDefault(queueName)?.Type ?? "direct";
                var routingKey = exchangeType == "fanout" ? "" : queueName;

                _channel.QueueBind(queue: queueName, exchange: queueName, routingKey: routingKey);
                Console.WriteLine($"[{_serviceInfo.Name}] Bound queue '{queueName}' to exchange '{queueName}'.");

                // Start listening for messages
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[{_serviceInfo.Name}] Received on '{queueName}' via '{ea.Exchange}': '{message}'");
                    Console.ResetColor();

                    HandleReceivedMessage(message);

                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };

                _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
                Console.WriteLine($"[{_serviceInfo.Name}] Listening for messages on '{queueName}'.");
            }
        }

        private void HandleReceivedMessage(string message)
        {
            if (_serviceInfo.Name == "AuthService" && message == "RegisterUser")
            {
                var responseEvent = "UserRegistered";
                // Publish to the 'system_events' exchange.
                PublishMessage(responseEvent, "system_events");
            }
        }

        public void PublishMessage(string message, string exchangeName)
        {
            if (_channel == null) return;

            var body = Encoding.UTF8.GetBytes(message);

            // Publish to the specified exchange. The routing key depends on the exchange type.
            // For fanout, it's ignored. For direct, it should be the queue name.
            var exchangeType = _rabbitMqConfig.GetValueOrDefault(exchangeName)?.Type ?? "direct";
            var routingKey = exchangeType == "fanout" ? "" : exchangeName;

            _channel.BasicPublish(exchange: exchangeName,
                                 routingKey: routingKey,
                                 basicProperties: null,
                                 body: body);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[{_serviceInfo.Name}] Published to exchange '{exchangeName}': '{message}'");
            Console.ResetColor();
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            GC.SuppressFinalize(this);
        }
    }
}