using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UDIE.EHLM.Core;

namespace UDIE.EHLM.Generator;

public class MicroservicesHoster
{
    private readonly MicroserviceConfig _config;
    private readonly List<Process> _runningProcesses = new();

    public MicroservicesHoster(MicroserviceConfig config)
    {
        _config = config;
    }

    public Task StartServicesAsync()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var executionDirectory = Path.GetDirectoryName(assemblyLocation);

        if (string.IsNullOrEmpty(executionDirectory))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: Could not determine the application's execution directory.");
            Console.ResetColor();
            throw new DirectoryNotFoundException("Could not determine the execution directory.");
        }

        var hostDllPath = Path.Combine(executionDirectory, "UDIE.EHLM.MicroserviceHost.dll");

        if (!File.Exists(hostDllPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: Microservice host executable not found at '{hostDllPath}'.");
            Console.WriteLine("Please ensure the solution has been built correctly.");
            Console.ResetColor();
            throw new FileNotFoundException("Microservice host not found.", hostDllPath);
        }

        if (_config.Microservices == null) return Task.CompletedTask;

        foreach (var serviceInfo in _config.Microservices)
        {
            // We create a new object that contains BOTH the specific service info
            // and the GLOBAL RabbitMQ configuration. This is what the host needs.
            var startupConfig = new
            {
                ServiceInfo = serviceInfo,
                RabbitMQConfig = _config.RabbitMQQueues
            };

            var configJson = JsonConvert.SerializeObject(startupConfig);
            var base64Config = Convert.ToBase64String(Encoding.UTF8.GetBytes(configJson));

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/home/jules/.dotnet/dotnet",
                    Arguments = $"\"{hostDllPath}\" {base64Config}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.OutputDataReceived += (sender, args) => Console.WriteLine($"[{serviceInfo.Name}]: {args.Data}");
            process.ErrorDataReceived += (sender, args) => Console.WriteLine($"[{serviceInfo.Name} ERROR]: {args.Data}");

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            _runningProcesses.Add(process);

            Console.WriteLine($"Starting '{serviceInfo.Name}' on port {serviceInfo.Port} (PID: {process.Id})...");
        }

        return Task.CompletedTask;
    }

    public async Task StartSimulatorAsync(string hostDllPath)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/home/jules/.dotnet/dotnet",
                Arguments = $"\"{hostDllPath}\" --run-simulator",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
        process.ErrorDataReceived += (sender, args) => Console.WriteLine($"[SIMULATOR ERROR]: {args.Data}");

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();
    }

    public void StopServices()
    {
        Console.WriteLine("\nShutting down all microservices...");
        foreach (var process in _runningProcesses)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(true);
                    Console.WriteLine($"Stopped process for PID {process.Id}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to stop process PID {process.Id}: {ex.Message}");
            }
        }
    }
}