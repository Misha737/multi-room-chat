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
    private ClientPool clientPool;
    private List<Room> rooms;
    public Server(string address, int port)
    {
        ServerIP = new IPEndPoint(IPAddress.Parse(address), port);
        ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ServerSocket.Bind(ServerIP);
        CancellationTokenSource = new();
        CancellationToken = CancellationTokenSource.Token;
        clientPool = new();
        rooms = new();
        acceptor = new(CancellationToken, clientPool);
    }

    public void Start()
    {
        ServerSocket.Listen(Backlog);
        acceptor.RunThread(ServerSocket);
    }
}
