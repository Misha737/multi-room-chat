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
    public List<ClientHandler> Clients { get; init; }
    private CancellationToken cancellationToken;
    private readonly object _lock;

    public Acceptor(CancellationToken ct, List<ClientHandler> clients, object _lock)
    {
        Clients = clients;
        cancellationToken = ct;
        this._lock = _lock;
        acceptorThread = new Thread(async () =>
        {
            await AcceptHandler();
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
                lock (_lock) {
                    Clients.Add(clientHandler);
                }
            }
            catch (SocketException)
            {
                continue;
            }
        }
    }
}