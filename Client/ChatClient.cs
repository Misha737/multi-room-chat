using Application;
using System.Net.Sockets;

namespace Client;

public class ChatClient
{
    private readonly Socket _socket;
    private readonly string _name;
    private int _roomId;

    private readonly Queue<byte[]> _sendQueue = new();
    private readonly object _sendQueueLock = new();
    private readonly SemaphoreSlim _sendAvailable = new SemaphoreSlim(0);

    private readonly Dictionary<string, FileOfferMessage> _pendingOffers = new();
    private readonly object _pendingOffersLock = new();

    private bool _running = true;

    public ChatClient(string host, int port, string name, int roomId)
    {
        _name = name;
        _roomId = roomId;
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.Connect(host, port);
    }


    public void Start()
    {
        SendExact(new JoinRoom(new ClientInfo(0, _roomId, _name)).Serialize());

        var (tag, payload) = ReceiveMessage();
        if (tag == Tag.OKStatus)
        {
            Console.WriteLine($"[OK] Joined room {_roomId} as \"{_name}\".");
        }
        else
        {
            ErrorStatus err = ErrorStatus.Deserialize(payload);
            Console.WriteLine($"[ERROR] {err.ErrorMessage}");
            return;
        }

        new Thread(RecvLoop) { IsBackground = true}.Start();
        new Thread(SendLoop) { IsBackground = true}.Start();

        CliLoop();
    }


    private void CliLoop()
    {
        PrintHelp();
        while (_running)
        {
            string? input = Console.ReadLine();
            if (input is null) continue;
            input = input.Trim();
            if (input.Length == 0) continue;

            if (input.StartsWith("/room "))
            {
                if (!int.TryParse(input[6..], out int newRoomId))
                { Console.WriteLine("[!] Usage: /room <id>"); continue; }

                _roomId = newRoomId;
                EnqueueSend(new ChangeRoomMessage(newRoomId).Serialize());
            }
            else if (input.StartsWith("/file "))
            {
                string path = input[6..].Trim();
                SendFile(path);
            }
            else if (input.StartsWith("/accept "))
            {
                string fileName = input[8..].Trim();
                EnqueueSend(new FileAcceptMessage(fileName).Serialize());
                lock (_pendingOffersLock) _pendingOffers.Remove(fileName);
            }
            else if (input.StartsWith("/reject "))
            {
                string fileName = input[8..].Trim();
                EnqueueSend(new FileRejectMessage(fileName).Serialize());
                lock (_pendingOffersLock) _pendingOffers.Remove(fileName);
                Console.WriteLine($"[!] Rejected '{fileName}'. You can still download it later with /download {fileName}");
            }
            else if (input.StartsWith("/download "))
            {
                string fileName = input[10..].Trim();
                EnqueueSend(new FileRequestMessage(fileName).Serialize());
            }
            else if (input == "/pending")
            {
                lock (_pendingOffersLock)
                {
                    if (_pendingOffers.Count == 0)
                        Console.WriteLine("[!] No pending file offers.");
                    else
                        foreach (var kv in _pendingOffers)
                            Console.WriteLine($"  {kv.Key}  ({kv.Value.FileSize / 1024.0:F1} KB) from {kv.Value.SenderName}");
                }
            }
            else if (input == "/help")
            {
                PrintHelp();
            }
            else if (input == "/quit")
            {
                _running = false;
                _socket.Close();
            }
            else
            {
                EnqueueSend(new ChatMessage(_name, input).Serialize());
            }
        }
    }

    private void SendFile(string path)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine($"[!] File not found: {path}");
            return;
        }

        try
        {
            byte[] data = File.ReadAllBytes(path);
            string fileName = Path.GetFileName(path);
            EnqueueSend(new FileDataMessage(fileName, data).Serialize());
            Console.WriteLine($"[*] Uploading {fileName} ({data.Length / 1024.0:F1} KB)…");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Could not read file: {ex.Message}");
        }
    }


    private void RecvLoop()
    {
        try
        {
            while (_running)
            {
                var (tag, payload) = ReceiveMessage();
                DispatchIncoming(tag, payload);
            }
        }
        catch
        {
            if (_running) Console.WriteLine("\n[!] Disconnected from server.");
            _running = false;
        }
    }

    private void DispatchIncoming(Tag tag, byte[] payload)
    {
        switch (tag)
        {
            case Tag.SystemMessage:
                Console.WriteLine($"\n[SYSTEM] {SystemMessage.Deserialize(payload).Content}");
                break;

            case Tag.ChatMessage:
                ChatMessage msg = ChatMessage.Deserialize(payload);
                Console.WriteLine($"\n{msg.SenderName}: {msg.Content}");
                break;

            case Tag.OKStatus:
                Console.WriteLine("[OK]");
                break;

            case Tag.ErrorStatus:
                Console.WriteLine($"[ERROR] {ErrorStatus.Deserialize(payload).ErrorMessage}");
                break;

            case Tag.FileOffer:
                HandleFileOffer(FileOfferMessage.Deserialize(payload));
                break;

            case Tag.FileData:
                HandleFileData(FileDataMessage.Deserialize(payload));
                break;
        }
    }

    private void HandleFileOffer(FileOfferMessage offer)
    {
        lock (_pendingOffersLock) _pendingOffers[offer.FileName] = offer;

        double sizeMb = offer.FileSize / (1024.0 * 1024.0);
        Console.WriteLine(
            $"\nCLIENT {offer.SenderName} wants to send {offer.FileName} file" +
            $" which size is {sizeMb:F2} MB, do you want to receive?");
        Console.WriteLine($"  → /accept {offer.FileName}   or   /reject {offer.FileName}");
    }

    private void HandleFileData(FileDataMessage fileData)
    {
        try
        {
            File.WriteAllBytes(fileData.FileName, fileData.Data);
            Console.WriteLine($"\nFile {fileData.FileName} has been downloaded");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Could not save file: {ex.Message}");
        }
    }

    private void SendLoop()
    {
        try
        {
            while (_running)
            {
                _sendAvailable.Wait();
                byte[] data;
                lock (_sendQueueLock)
                {
                    if (_sendQueue.Count == 0) continue;
                    data = _sendQueue.Dequeue();
                }
                SendExact(data);
            }
        }
        catch {  }
    }

    private void EnqueueSend(byte[] data)
    {
        lock (_sendQueueLock) _sendQueue.Enqueue(data);
        _sendAvailable.Release();
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
        byte[] buf = new byte[count];
        int received = 0;
        while (received < count)
        {
            int n = _socket.Receive(buf, received, count - received, SocketFlags.None);
            if (n == 0) throw new EndOfStreamException("Server closed connection.");
            received += n;
        }
        return buf;
    }

    private void SendExact(byte[] data)
    {
        int sent = 0;
        while (sent < data.Length)
        {
            int n = _socket.Send(data, sent, data.Length - sent, SocketFlags.None);
            if (n == 0) throw new EndOfStreamException("Server closed connection.");
            sent += n;
        }
    }


    private static void PrintHelp()
    {
        Console.WriteLine("""

        ╔══════════════════════════════════════════════╗
        ║           Chat Client – Commands             ║
        ╠══════════════════════════════════════════════╣
        ║  <text>              Send a chat message     ║
        ║  /room <id>          Switch to another room  ║
        ║  /file <path>        Upload a file           ║
        ║  /accept <filename>  Accept a file offer     ║
        ║  /reject <filename>  Reject a file offer     ║
        ║  /download <file>    Postponed file download ║
        ║  /pending            List pending offers     ║
        ║  /help               Show this help          ║
        ║  /quit               Disconnect              ║
        ╚══════════════════════════════════════════════╝
        """);
    }
}
