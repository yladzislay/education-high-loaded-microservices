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

        var microservices = await MicroservicesGenerator.GenerateMicroservicesAsync(configPath);

        Console.WriteLine("Generated microservices configuration:");
        foreach (var service in microservices)
        {
            Console.WriteLine($"- Service: {service.Name}, Port: {service.Port}");
        }

        Console.WriteLine("\n-----------------------------------\n");

        var hoster = new MicroservicesHoster(microservices);
        await hoster.StartServicesAsync();

        Console.WriteLine("\nAll microservices started. Press any key to shut down.");
        Console.ReadKey();

        hoster.StopServices();
    }
}