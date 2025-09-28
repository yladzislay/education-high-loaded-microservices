using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using UDIE.EHLM.Core;

namespace UDIE.EHLM.Generator;

public static class MicroservicesGenerator
{
    public static async Task<List<MicroserviceInfo>> GenerateMicroservicesAsync(string configFile)
    {
        var json = await File.ReadAllTextAsync(configFile);
        var microserviceConfig = JsonConvert.DeserializeObject<MicroserviceConfig>(json);
        if (microserviceConfig?.Microservices == null)
        {
            return new List<MicroserviceInfo>();
        }

        foreach (var microserviceInfo in microserviceConfig.Microservices)
        {
            microserviceInfo.Port = GetAvailablePort();
        }

        return microserviceConfig.Microservices;
    }

    private static int GetAvailablePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
