using Server.Messages;

namespace Server;

public class Room
{
    public int Id { get; init; }
    public string Name { get; init; }

    private readonly List<ClientHandler> _clients = new();
    private readonly object _clientsLock = new();

    public int ClientCount { get { lock (_clientsLock) return _clients.Count; } }

    private readonly Queue<(ClientHandler Sender, Message Msg)> _messageQueue = new();
    private readonly object _queueLock = new();
    private readonly SemaphoreSlim _messageAvailable = new SemaphoreSlim(0);

    private readonly Dictionary<string, byte[]> _fileStorage = new();
    private readonly object _fileStorageLock = new();

    public Room(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public void Start()
    {
        Thread broadcastThread = new Thread(BroadcastLoop) { IsBackground = true };
        broadcastThread.Start();
    }

    public void AddClient(ClientHandler client)
    {
        lock (_clientsLock) _clients.Add(client);
        Broadcast(new SystemMessage($"CLIENT {client.Name} JOINED ROOM {Id}"), excludeSender: null);
        Console.WriteLine($"[Room {Id}] CLIENT {client.Name} JOINED");
    }

    public void RemoveClient(ClientHandler client)
    {
        lock (_clientsLock) _clients.Remove(client);
        Broadcast(new SystemMessage($"CLIENT {client.Name} LEFT ROOM {Id}"), excludeSender: client);
        Console.WriteLine($"[Room {Id}] CLIENT {client.Name} LEFT");
    }

    public void EnqueueMessage(ClientHandler sender, Message message)
    {
        lock (_queueLock) _messageQueue.Enqueue((sender, message));
        _messageAvailable.Release();
    }

    private void BroadcastLoop()
    {
        while (true)
        {
            _messageAvailable.Wait();

            (ClientHandler sender, Message msg) item;
            lock (_queueLock)
            {
                if (_messageQueue.Count == 0) continue;
                item = _messageQueue.Dequeue();
            }

            ProcessMessage(item.sender, item.msg);
        }
    }


    private void ProcessMessage(ClientHandler sender, Message message)
    {
        switch (message)
        {
            case ChatMessage chatMsg:
                Console.WriteLine($"[Room {Id}] {sender.Name}: {chatMsg.Content}");
                Broadcast(chatMsg, excludeSender: sender);
                break;

            case FileDataMessage fileData:
                lock (_fileStorageLock) _fileStorage[fileData.FileName] = fileData.Data;
                Console.WriteLine($"[Room {Id}] CLIENT {sender.Name} sends a file {fileData.FileName}...");
                sender.EnqueueSend(new OKStatus().Serialize());
                var offer = new FileOfferMessage(sender.Name, fileData.FileName, fileData.Data.Length);
                Broadcast(offer, excludeSender: sender);
                break;

            case FileAcceptMessage fileAccept:
                SendStoredFile(sender, fileAccept.FileName);
                break;

            case FileRejectMessage fileReject:
                Console.WriteLine($"[Room {Id}] CLIENT {sender.Name} rejected file {fileReject.FileName}");
                break;

            case FileRequestMessage fileRequest:
                Console.WriteLine($"[Room {Id}] CLIENT {sender.Name} requests stored file {fileRequest.FileName}");
                SendStoredFile(sender, fileRequest.FileName);
                break;
        }
    }

    private void SendStoredFile(ClientHandler recipient, string fileName)
    {
        byte[]? data;
        lock (_fileStorageLock) _fileStorage.TryGetValue(fileName, out data);

        if (data is null)
        {
            recipient.EnqueueSend(new ErrorStatus($"File '{fileName}' not found in room storage.").Serialize());
            return;
        }

        recipient.EnqueueSend(new FileDataMessage(fileName, data).Serialize());
        Console.WriteLine($"[Room {Id}] File {fileName} has been downloaded by {recipient.Name}");
    }


    private void Broadcast(Message message, ClientHandler? excludeSender)
    {
        byte[] raw = message.Serialize();
        lock (_clientsLock)
        {
            foreach (ClientHandler client in _clients)
            {
                if (client != excludeSender)
                    client.EnqueueSend(raw);
            }
        }
    }

    public List<string> GetStoredFiles()
    {
        lock (_fileStorageLock) return new List<string>(_fileStorage.Keys);
    }
}
