using System.Net.Sockets;

namespace Server;

internal class Acceptor
{
    private readonly Socket _serverSocket;
    private readonly List<Room> _rooms;
    private readonly ClientPool _clientPool;
    private readonly CancellationToken _ct;

    public Acceptor(Socket serverSocket, List<Room> rooms, ClientPool clientPool, CancellationToken ct)
    {
        _serverSocket = serverSocket;
        _rooms = rooms;
        _clientPool = clientPool;
        _ct = ct;
    }

    public void Start()
    {
        new Thread(AcceptLoop) { IsBackground = true }.Start();
    }

    private void AcceptLoop()
    {
        Console.WriteLine("[Acceptor] Listening for connections…");
        while (!_ct.IsCancellationRequested)
        {
            try
            {
                Socket clientSocket = _serverSocket.Accept();
                Console.WriteLine($"[Acceptor] New connection from {clientSocket.RemoteEndPoint}");

                ClientHandler handler = new ClientHandler(clientSocket, _rooms, _ct);
                _clientPool.Add(handler);
                handler.Start();
            }
            catch (SocketException)
            {
                if (!_ct.IsCancellationRequested)
                    Console.WriteLine("[Acceptor] Accept error, retrying…");
            }
        }
    }
}
