using System.IO;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace UDIE.EHLM.Generator;

using System.Collections.Generic;

public static class MicroservicesGenerator
{
    public static List<MicroserviceInfo> GenerateMicroservices(string configFile)
    {
        var json = File.ReadAllText(configFile);
        var microserviceConfig = JsonConvert.DeserializeObject<MicroserviceConfig>(json);
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
