using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Server;

internal class Acceptor
{
    private Thread acceptorThread;
    public Socket ServerSocket { get; set; }

    public Acceptor(Socket serverSocket, CancellationToken ct)
    {
        acceptorThread = new Thread(AcceptHandler);
    }

    public void Run()
    {
        acceptorThread.Start();
    }

    private void AcceptHandler()
    {

    }
}