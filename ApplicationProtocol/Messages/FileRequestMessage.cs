using System.Text;

namespace Application;

public class FileRequestMessage : Message
{
    public string FileName { get; set; }

    public FileRequestMessage(string fileName) : base(Tag.FileRequest)
        => FileName = fileName;

    public static FileRequestMessage Deserialize(ReadOnlySpan<byte> data)
    {
        int len = BitConverter.ToInt32(data);
        string fileName = Encoding.UTF8.GetString(data.Slice(4, len));
        return new FileRequestMessage(fileName);
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
