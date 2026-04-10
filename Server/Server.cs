using System.Net;
using System.Net.Sockets;

namespace Server;

public class ChatServer
{
    private readonly IPEndPoint _endpoint;
    private readonly Socket _serverSocket;
    private readonly List<Room> _rooms;
    private readonly ClientPool _clientPool = new();
    private readonly CancellationTokenSource _cts = new();

    public ChatServer(string address, int port)
    {
        _endpoint = new IPEndPoint(IPAddress.Parse(address), port);
        _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _serverSocket.Bind(_endpoint);

        _rooms = new List<Room>
        {
            new Room(1, "General"),
            new Room(2, "Random")
        };
    }

    public void Start()
    {
        foreach (Room room in _rooms)
            room.Start();

        _serverSocket.Listen(10);
        Console.WriteLine($"[Server] Listening on {_endpoint}");

        new Acceptor(_serverSocket, _rooms, _clientPool, _cts.Token).Start();

        new Thread(CliLoop) { IsBackground = false }.Start();
    }

    public void Stop()
    {
        _cts.Cancel();
        _serverSocket.Close();
    }


    private void CliLoop()
    {
        Console.WriteLine("[Server CLI] Commands: /rooms, /quit");
        while (true)
        {
            string? input = Console.ReadLine();
            if (input is null) continue;

            if (input.Trim() == "/rooms")
            {
                foreach (Room r in _rooms)
                    Console.WriteLine($"  Room {r.Id} \"{r.Name}\" – {r.ClientCount} client(s)");
            }
            else if (input.Trim() == "/quit")
            {
                Stop();
                break;
            }
        }
    }
}
