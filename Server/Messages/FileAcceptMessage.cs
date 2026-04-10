using System.Text;

namespace Server.Messages;

public class FileAcceptMessage : Message
{
    public string FileName { get; set; }

    public FileAcceptMessage(string fileName) : base(Tag.FileAccept)
        => FileName = fileName;

    public static FileAcceptMessage Deserialize(ReadOnlySpan<byte> data)
    {
        int len = BitConverter.ToInt32(data);
        string fileName = Encoding.UTF8.GetString(data.Slice(4, len));
        return new FileAcceptMessage(fileName);
    }

    public override byte[] SerializePayload()
    {
        byte[] fileNameBytes = Encoding.UTF8.GetBytes(FileName);
        List<byte> bytes =
        [
            ..BitConverter.GetBytes(fileNameBytes.Length),
            ..fileNameBytes
        ];
        return bytes.ToArray();
    }
}
