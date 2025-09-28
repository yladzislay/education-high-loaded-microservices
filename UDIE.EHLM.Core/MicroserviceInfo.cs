namespace UDIE.EHLM.Core;

public class MicroserviceInfo
{
    public string Name { get; set; } = string.Empty;
    public int Port { get; set; }

    public List<string> Events { get; set; } = new();
    public List<string> Commands { get; set; } = new();
    public List<string> Queries { get; set; } = new();
    public List<string> Notifications { get; set; } = new();

    public List<string> OutgoingQueues { get; set; } = new();
    public List<string> IncomingQueues { get; set; } = new();
}