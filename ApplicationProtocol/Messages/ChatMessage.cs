using System.Text;

namespace Application;

public class ChatMessage : Message
{
    public string SenderName { get; set; }
    public string Content { get; set; }

    public ChatMessage(string senderName, string content) : base(Tag.ChatMessage)
    {
        SenderName = senderName;
        Content = content;
    }

    public static ChatMessage Deserialize(ReadOnlySpan<byte> data)
    {
        int senderNameLen = BitConverter.ToInt32(data);
        string senderName = Encoding.UTF8.GetString(data.Slice(4, senderNameLen));
        int offset = 4 + senderNameLen;
        int contentLen = BitConverter.ToInt32(data.Slice(offset));
        string content = Encoding.UTF8.GetString(data.Slice(offset + 4, contentLen));
        return new ChatMessage(senderName, content);
    }

    public override byte[] SerializePayload()
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes(SenderName);
        byte[] contentBytes = Encoding.UTF8.GetBytes(Content);
        List<byte> bytes =
        [
            ..BitConverter.GetBytes(nameBytes.Length),
            ..nameBytes,
            ..BitConverter.GetBytes(contentBytes.Length),
            ..contentBytes
        ];
        return bytes.ToArray();
    }
}
