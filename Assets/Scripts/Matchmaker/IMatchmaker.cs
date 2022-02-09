using System.Threading.Tasks;

public enum Mode { casual, rank };

public interface ITicket
{
    string Id { get; }
}

public struct PendingTicket : ITicket
{
    public string Id { get; private set; }
    public PendingTicket(string id)
    {
        Id = id;
    }
}

public struct ReadyTicket : ITicket
{
    public string Id { get; private set; }
    public string Ip { get; private set; }
    public int Port { get; private set; }
    public ReadyTicket(string id, string serverIp, int serverPort)
    {
        Id = id;
        Ip = serverIp;
        Port = serverPort;
    }
}

public interface IMatchmaker
{
    Task<PendingTicket> CreateTicket(Mode mode);
    Task<ReadyTicket> ResolveTicket(PendingTicket ticket, int timeout);
    Task DeleteTicket(ITicket ticket);
}