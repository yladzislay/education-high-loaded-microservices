using System.Reflection;
using UDIE.EHLM.Core;

namespace UDIE.EHLM.Generator;

public class Program
{
    public static async Task Main(string[] args)
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var executionDirectory = Path.GetDirectoryName(assemblyLocation);

        if (string.IsNullOrEmpty(executionDirectory))
        {
            Console.WriteLine("Error: Could not determine the application's execution directory.");
            return;
        }

        var configPath = Path.Combine(executionDirectory, "microservices.json");

        if (!File.Exists(configPath))
        {
            Console.WriteLine($"Error: Configuration file not found at '{configPath}'.");
            return;
        }

        var microserviceConfig = await MicroservicesGenerator.GenerateMicroservicesAsync(configPath);
        if (microserviceConfig == null || microserviceConfig.Microservices == null)
        {
            Console.WriteLine("Error: Failed to generate microservice configuration.");
            return;
        }

        Console.WriteLine("Generated microservices configuration:");
        foreach (var service in microserviceConfig.Microservices)
        {
            Console.WriteLine($"- Service: {service.Name}, Port: {service.Port}");
        }

        Console.WriteLine("\n-----------------------------------\n");

        var hoster = new MicroservicesHoster(microserviceConfig);
        await hoster.StartServicesAsync();

        Console.WriteLine("\nAll microservices started.");

        // Wait a moment for services to initialize their listeners
        await Task.Delay(5000);

        // Run the simulator in a separate process
        Console.WriteLine("\n-----------------------------------\n");
        Console.WriteLine("Starting simulator to send command...");

        var hostDllPath = Path.Combine(executionDirectory, "UDIE.EHLM.MicroserviceHost.dll");
        var simulatorHoster = new MicroservicesHoster(microserviceConfig);
        await simulatorHoster.StartSimulatorAsync(hostDllPath);


        Console.WriteLine("\nSimulation finished. Press any key to shut down all services.");
        Console.ReadKey();

        hoster.StopServices();
    }
}