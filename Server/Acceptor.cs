using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace Server;

internal class Acceptor
{
    public const int Backlog = 10;

    public Socket? ServerSocket { get; set; }
    public ClientPool ClientPool { get; init; }
    private CancellationToken cancellationToken;

    public Acceptor(CancellationToken ct, ClientPool clientPool)
    {
        ClientPool = clientPool;
        cancellationToken = ct;
    }

    public void RunThread(Socket serverSocket)
    {
        ServerSocket = serverSocket;
        Task.Run(() => AcceptHandler());
    }

    private async Task AcceptHandler()
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Socket clientSocket;
            try
            {
                clientSocket = await ServerSocket!.AcceptAsync();
                ClientHandler clientHandler = new ClientHandler(clientSocket, cancellationToken);
                ClientPool.PushClient(clientHandler);
            }
            catch (SocketException)
            {
                continue;
            }
        }
    }
}