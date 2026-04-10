using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server;

internal class Server
{
    public const int Backlog = 10;
    public IPEndPoint ServerIP { get; init; }
    private Socket ServerSocket;
    private CancellationTokenSource CancellationTokenSource;
    private CancellationToken CancellationToken;
    private Acceptor acceptor;
    private List<ClientHandler> clients;
    private readonly object _lock = new object();
    public Server(string address, int port)
    {
        ServerIP = new IPEndPoint(IPAddress.Parse(address), port);
        ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ServerSocket.Bind(ServerIP);
        CancellationTokenSource = new CancellationTokenSource();
        CancellationToken = CancellationTokenSource.Token;
        acceptor = new Acceptor(CancellationToken, clients);
    }

    public void Start()
    {
        ServerSocket.Listen(Backlog);
        acceptor.RunThread(ServerSocket);
    }
}
