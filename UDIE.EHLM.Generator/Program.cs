namespace UDIE.EHLM.Generator;

public class Program
{
    public static async Task Main(string[] args)
    {
        var generator = MicroservicesGenerator.CreateInstance();
        var microservices = await generator.GenerateMicroservicesAsync(3);

        var simulator = new MicroservicesSimulator();
        await simulator.SimulateInteractionAsync(microservices);
    }
}