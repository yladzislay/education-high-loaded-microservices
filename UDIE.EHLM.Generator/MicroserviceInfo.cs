namespace UDIE.EHLM.Generator;

public class MicroserviceInfo
{
    public string Name { get; set; }
    public int Port { get; set; }
    
    public List<string> Events { get; set; }
    public List<string> Commands { get; set; }
    public List<string> Queries { get; set; }
    public List<string> Notifications { get; set; }
    
    public List<string> OutgoingQueues { get; set; }
    public List<string> IncomingQueues { get; set; }
}