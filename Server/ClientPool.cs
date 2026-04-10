using System;
using System.Collections.Generic;
using System.Text;

namespace Server;

public class ClientPool
{
    private readonly object _lock = new();
    private List<ClientHandler> clients = new List<ClientHandler>();

    public ClientPool()
    {

    }

    public void PushClient(ClientHandler newClient)
    {
        lock (_lock)
        {
            clients.Add(newClient);
        }
    }
}
