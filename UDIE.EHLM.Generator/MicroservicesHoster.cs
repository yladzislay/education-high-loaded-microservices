using Microsoft.Extensions.Hosting;

namespace UDIE.EHLM.Generator;

public class MicroservicesHoster
{
    public void StartMicroservices(List<MicroserviceInfo> microservices)
    {
        foreach (var microservice in microservices)
        {
            microservice.Host.Start();
        }
    }

    public void StopMicroservices(List<Microservice> microservices)
    {
        foreach (var microservice in microservices)
        {
            microservice.Host.StopAsync();
        }
    }

    private IHost CreateHost(string port) =>
        Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls($"http://localhost:{port}");
                webBuilder.Configure(app =>
                {
                    app.Run(async context =>
                    {
                        await context.Response.WriteAsync($"Microservice running on port {port}");
                    });
                });
            })
            .Build();
}