namespace Server;

public class ClientPool
{
    private readonly List<ClientHandler> _clients = new();
    private readonly object _lock = new();

    public void Add(ClientHandler client)
    {
        lock (_lock) _clients.Add(client);
    }

    public void Remove(ClientHandler client)
    {
        lock (_lock) _clients.Remove(client);
    }

    public int Count { get { lock (_lock) return _clients.Count; } }
}
