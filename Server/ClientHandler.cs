using Server.Messages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Sockets;
using System.Text;

namespace Server;

public class ClientHandler
{
    public Socket ClientSocket { get; init; }
    private CancellationToken cancellationToken;
    private Thread ReceiveThread;
    private Thread SendThread;
    private List<Room> rooms;
    private Room? _room;
    public ClientHandler(Socket clientSocket, List<Room> rooms, CancellationToken cancellationToken)
    {
        this.ClientSocket = clientSocket;
        this.cancellationToken = cancellationToken;
        this.rooms = rooms;
        ReceiveThread = new Thread(ReceivingThread);
        SendThread = new Thread(SendingThread);
    }

    public void Start()
    {
        ReceiveThread.Start();
        SendThread.Start();
    }

    private async Task ReceivingThread()
    {
        while(!cancellationToken.IsCancellationRequested)
        {
            Tag tag = await ReceiveTag();
            int length = await ReceiveLength();
            byte[] payload = await ReceiveExact(length);

            if (tag == Tag.JoinRoom)
            {
                JoinRoom joinRoom = JoinRoom.Deserialize(payload);

                foreach (Room room in rooms)
                {
                    if (room.Id == joinRoom.ClientInfo.RoomId)
                    {
                        _room = room;
                        OKStatus okStatus = new OKStatus();
                        await SendExact(okStatus.Serialize());
                        break;
                    }
                }

                ErrorStatus errorStatus = new ErrorStatus("Requested room not found.");
                await SendExact(errorStatus.Serialize());

            }
            else
            {
                if(_room == null)
                {
                    ErrorStatus errorStatus = new ErrorStatus("You need to join a room first.");
                    await SendExact(errorStatus.Serialize());
                }
            }
        }
    }

    private void SendingThread()
    {
        while (!cancellationToken.IsCancellationRequested)
        {
        }
    }

    private async Task<Tag> ReceiveTag() => (Tag)(await ReceiveExact(1))[0];

    private async Task<int> ReceiveLength() => BitConverter.ToInt32(await ReceiveExact(4));

    private async Task<byte[]> ReceiveExact(int byteCount)
    {
        byte[] buffer = new byte[byteCount];
        int received = 0;
        while (received < byteCount)
        {
            int bytesRead = await ClientSocket.ReceiveAsync(new ArraySegment<byte>(buffer, received, byteCount - received), SocketFlags.None, cancellationToken);
            if (bytesRead == 0)
            {
                throw new SocketException();
            }
            received += bytesRead;
        }

        return buffer;
    }

    private async Task SendExact(byte[] data)
    {
        int sent = 0;
        while (sent < data.Length)
        {
            int bytesSent = await ClientSocket.SendAsync(new ArraySegment<byte>(data, sent, data.Length - sent), SocketFlags.None, cancellationToken);
            if (bytesSent == 0)
            {
                throw new SocketException();
            }
            sent += bytesSent;
        }
    }
}