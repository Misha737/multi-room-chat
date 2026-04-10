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
        int nameLen = BitConverter.ToInt32(raw.Slice(8));
        string name = Encoding.UTF8.GetString(raw.Slice(12, nameLen));
        return new ClientInfo(id, roomId, name);
    }

    public byte[] Serialize()
    {
        int nameLen = Encoding.UTF8.GetByteCount(Name);
        List<byte> bytes =
        [
            ..BitConverter.GetBytes(Id),
            ..BitConverter.GetBytes(RoomId),
            ..BitConverter.GetBytes(nameLen),
            ..Encoding.UTF8.GetBytes(Name)
        ];
        return bytes.ToArray();
    }
}
