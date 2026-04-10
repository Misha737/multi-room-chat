using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client;

internal class Client
{
    private Socket serverSocket;
    IPEndPoint serverEndPoint;
    public Client (string address, int port)
    {
        serverEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public async Task Connect()
    {
        await serverSocket.ConnectAsync(serverEndPoint);


    }
}