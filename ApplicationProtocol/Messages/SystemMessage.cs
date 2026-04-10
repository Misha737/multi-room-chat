using System.Text;

namespace Application;

public class SystemMessage : Message
{
    public string Content { get; set; }

    public SystemMessage(string content) : base(Tag.SystemMessage)
        => Content = content;

    public static SystemMessage Deserialize(ReadOnlySpan<byte> data)
    {
        int len = BitConverter.ToInt32(data);
        string text = Encoding.UTF8.GetString(data.Slice(4, len));
        return new SystemMessage(text);
    }

    public override byte[] SerializePayload()
    {
        byte[] contentBytes = Encoding.UTF8.GetBytes(Content);
        List<byte> bytes =
        [
            ..BitConverter.GetBytes(contentBytes.Length),
            ..contentBytes
        ];
        return bytes.ToArray();
    }
}
