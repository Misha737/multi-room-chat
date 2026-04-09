using System;
using System.Collections.Generic;
using System.Text;

namespace Server;

public class ClientInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ClientInfo(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public static ClientInfo Deserialize(ReadOnlySpan<byte> raw)
    {
        int id = BitConverter.ToInt32(raw);
        string name = Encoding.UTF8.GetString(raw.Slice(4));
        return new ClientInfo(id, name);
    }

    public byte[] Serialize()
    {
        List<byte> bytes = [.. BitConverter.GetBytes(Id), .. Encoding.UTF8.GetBytes(Name)];
        return bytes.ToArray();
    }
}
