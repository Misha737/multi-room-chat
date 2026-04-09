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
    public Server(string address, int port)
    {
        ServerIP = new IPEndPoint(IPAddress.Parse(address), port);
        ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ServerSocket.Bind(ServerIP);
        CancellationTokenSource = new CancellationTokenSource();
        CancellationToken = CancellationTokenSource.Token;
    }

    public void Start()
    {
        ServerSocket.Listen(Backlog);
        while (!CancellationToken.IsCancellationRequested)
        {
            Socket clientSocket;
            try
            {
                clientSocket = ServerSocket.Accept();
            }
            catch (SocketException)
            {
                continue;
            }
        }
    }
}
