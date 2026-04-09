using System;
using System.Collections.Generic;
using System.Text;

namespace Server;

public class ClientInfo
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string Name { get; set; }
    public ClientInfo(int id, int roomId, string name)
    {
        Id = id;
        RoomId = roomId;
        Name = name;
    }

    public static ClientInfo Deserialize(ReadOnlySpan<byte> raw)
    {
        int id = BitConverter.ToInt32(raw);
        int roomId = BitConverter.ToInt32(raw.Slice(4));
        string name = Encoding.UTF8.GetString(raw.Slice(8));

        return new ClientInfo(id, roomId, name);
    }

    public byte[] Serialize()
    {
        List<byte> bytes = [.. BitConverter.GetBytes(Id), .. BitConverter.GetBytes(RoomId), .. Encoding.UTF8.GetBytes(Name)];
        return bytes.ToArray();
    }
}
