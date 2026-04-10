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

    private Thread acceptorThread;
    public Socket? ServerSocket { get; set; }
    public ClientPool ClientPool { get; init; }
    private CancellationToken cancellationToken;

    public Acceptor(CancellationToken ct, ClientPool clientPool)
    {
        ClientPool = clientPool;
        cancellationToken = ct;
        acceptorThread = new Thread( () =>
        {
            AcceptHandler().Wait();
        });

    }

    public void RunThread(Socket serverSocket)
    {
        ServerSocket = serverSocket;
        acceptorThread.Start();
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