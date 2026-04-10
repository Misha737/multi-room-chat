using Application;
using System.Net.Sockets;

namespace Server;

public class ClientHandler
{
    public Socket ClientSocket { get; init; }
    public int ClientId { get; private set; }
    public string Name { get; private set; } = "Unknown";

    private static int _nextId = 0;

    private readonly List<Room> _rooms;
    private Room? _currentRoom;
    private readonly CancellationToken _ct;

    private readonly Queue<byte[]> _sendQueue = new();
    private readonly object _sendQueueLock = new();
    private readonly SemaphoreSlim _sendAvailable = new SemaphoreSlim(0);

    public ClientHandler(Socket clientSocket, List<Room> rooms, CancellationToken ct)
    {
        ClientSocket = clientSocket;
        _rooms = rooms;
        _ct = ct;
        ClientId = Interlocked.Increment(ref _nextId);
    }

    public void Start()
    {
        new Thread(ReceiveLoop) { IsBackground = false }.Start();
        new Thread(SendLoop) { IsBackground = false }.Start();
    }

    public void EnqueueSend(byte[] data)
    {
        lock (_sendQueueLock) _sendQueue.Enqueue(data);
        _sendAvailable.Release();
    }

    private void SendLoop()
    {
        try
        {
            while (!_ct.IsCancellationRequested)
            {
                _sendAvailable.Wait(_ct);
                byte[] data;
                lock (_sendQueueLock)
                {
                    if (_sendQueue.Count == 0) continue;
                    data = _sendQueue.Dequeue();
                }
                SendExact(data);
            }
        }
        catch { }
    }

    private void ReceiveLoop()
    {
        try
        {
            PerformJoinHandshake();

            while (!_ct.IsCancellationRequested)
            {
                var (tag, payload) = ReceiveMessage();
                HandleMessage(tag, payload);
            }
        }
        catch (Exception ex) when (ex is SocketException or OperationCanceledException or EndOfStreamException)
        {
            Console.WriteLine($"[Server] CLIENT {Name} disconnected.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] CLIENT {Name} error: {ex.Message}");
        }
        finally
        {
            _currentRoom?.RemoveClient(this);
            try { ClientSocket.Close(); } catch { }
        }
    }

    private void PerformJoinHandshake()
    {
        var (tag, payload) = ReceiveMessage();

        if (tag != Tag.JoinRoom)
        {
            SendExact(new ErrorStatus("First message must be JoinRoom.").Serialize());
            throw new Exception("Protocol error: expected JoinRoom.");
        }

        var joinRoom = JoinRoom.Deserialize(payload);
        Name = joinRoom.ClientInfo.Name;
        int requestedRoomId = joinRoom.ClientInfo.RoomId;

        Room? room = _rooms.FirstOrDefault(r => r.Id == requestedRoomId);
        if (room is null)
        {
            SendExact(new ErrorStatus($"Room {requestedRoomId} does not exist.").Serialize());
            throw new Exception($"Room {requestedRoomId} not found.");
        }

        _currentRoom = room;
        SendExact(new OKStatus().Serialize());
        room.AddClient(this);
    }

    private void HandleMessage(Tag tag, byte[] payload)
    {
        if (_currentRoom is null)
        {
            EnqueueSend(new ErrorStatus("Join a room first.").Serialize());
            return;
        }

        switch (tag)
        {
            case Tag.ChatMessage:
                _currentRoom.EnqueueMessage(this, ChatMessage.Deserialize(payload));
                break;

            case Tag.ChangeRoom:
                HandleChangeRoom(ChangeRoomMessage.Deserialize(payload).NewRoomId);
                break;

            case Tag.FileData:
                _currentRoom.EnqueueMessage(this, FileDataMessage.Deserialize(payload));
                break;

            case Tag.FileAccept:
                _currentRoom.EnqueueMessage(this, FileAcceptMessage.Deserialize(payload));
                break;

            case Tag.FileReject:
                _currentRoom.EnqueueMessage(this, FileRejectMessage.Deserialize(payload));
                break;

            case Tag.FileRequest:
                _currentRoom.EnqueueMessage(this, FileRequestMessage.Deserialize(payload));
                break;

            default:
                EnqueueSend(new ErrorStatus($"Unknown tag: {tag}").Serialize());
                break;
        }
    }

    private void HandleChangeRoom(int newRoomId)
    {
        Room? newRoom = _rooms.FirstOrDefault(r => r.Id == newRoomId);
        if (newRoom is null)
        {
            EnqueueSend(new ErrorStatus($"Room {newRoomId} does not exist.").Serialize());
            return;
        }

        _currentRoom?.RemoveClient(this);
        _currentRoom = newRoom;
        EnqueueSend(new OKStatus().Serialize());
        newRoom.AddClient(this);
    }

    private (Tag tag, byte[] payload) ReceiveMessage()
    {
        Tag tag = (Tag)ReceiveExact(1)[0];
        int length = BitConverter.ToInt32(ReceiveExact(4));
        byte[] payload = length > 0 ? ReceiveExact(length) : Array.Empty<byte>();
        return (tag, payload);
    }

    private byte[] ReceiveExact(int count)
    {
        byte[] buffer = new byte[count];
        int received = 0;
        while (received < count)
        {
            int n = ClientSocket.Receive(buffer, received, count - received, SocketFlags.None);
            if (n == 0) throw new EndOfStreamException("Connection closed by client.");
            received += n;
        }
        return buffer;
    }

    private void SendExact(byte[] data)
    {
        int sent = 0;
        while (sent < data.Length)
        {
            int n = ClientSocket.Send(data, sent, data.Length - sent, SocketFlags.None);
            if (n == 0) throw new EndOfStreamException("Connection closed by client.");
            sent += n;
        }
    }
}
