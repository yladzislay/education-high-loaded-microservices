using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UDIE.EHLM.Core;

namespace UDIE.EHLM.Generator;

public class MicroservicesHoster
{
    private readonly List<MicroserviceInfo> _microservices;
    private readonly List<Process> _runningProcesses = new();

    public MicroservicesHoster(List<MicroserviceInfo> microservices)
    {
        _microservices = microservices;
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

        foreach (var serviceInfo in _microservices)
        {
            var configJson = JsonConvert.SerializeObject(serviceInfo);
            var base64Config = Convert.ToBase64String(Encoding.UTF8.GetBytes(configJson));

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
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