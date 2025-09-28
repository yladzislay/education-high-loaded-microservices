using System.Text;
using Newtonsoft.Json;
using UDIE.EHLM.Core;
using UDIE.EHLM.MicroserviceHost;

// Check for simulator mode
if (args.Length > 0 && args[0] == "--run-simulator")
{
    await MicroservicesSimulator.Run();
    return;
}

// --- Standard Microservice Startup Logic ---

var builder = WebApplication.CreateBuilder(args);

if (args.Length == 0)
{
    throw new ArgumentException("Microservice startup configuration (Base64) is required as a command-line argument.");
}

var base64EncodedConfig = args[0];
var jsonConfig = Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedConfig));
var startupConfig = JsonConvert.DeserializeObject<StartupConfig>(jsonConfig);

if (startupConfig == null)
{
    throw new InvalidOperationException("Failed to deserialize microservice startup configuration.");
}

var microserviceInfo = startupConfig.ServiceInfo;
var rabbitMqConfig = startupConfig.RabbitMQConfig;

// Configure the web server to listen on the specified port
builder.WebHost.UseUrls($"http://localhost:{microserviceInfo.Port}");

var app = builder.Build();

// Initialize RabbitMQ Service with full configuration
var rabbitMqService = new RabbitMqService(microserviceInfo, rabbitMqConfig);
rabbitMqService.Initialize();

// Ensure RabbitMQ connection is closed on application shutdown
app.Lifetime.ApplicationStopping.Register(() =>
{
    rabbitMqService.Dispose();
    Console.WriteLine($"[{microserviceInfo.Name}] RabbitMQ connection closed.");
});


app.MapGet("/", () => $"Hello from {microserviceInfo.Name}!");

Console.WriteLine($"'{microserviceInfo.Name}' is running and listening on http://localhost:{microserviceInfo.Port}");

app.Run();