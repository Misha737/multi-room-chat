using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Sockets;
using System.Text;

namespace Server;

public class ClientHandler
{
    public Socket ClientSocket { get; init; }
    private CancellationToken cancellationToken;
    public ClientHandler(Socket clientSocket, CancellationToken cancellationToken)
    {
        this.ClientSocket = clientSocket;
        this.cancellationToken = cancellationToken;
    }
}
