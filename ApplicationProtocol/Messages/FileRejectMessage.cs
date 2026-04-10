using System.Text;

namespace Application;

public class FileRejectMessage : Message
{
    public string FileName { get; set; }

    public FileRejectMessage(string fileName) : base(Tag.FileReject)
        => FileName = fileName;

    public static FileRejectMessage Deserialize(ReadOnlySpan<byte> data)
    {
        int len = BitConverter.ToInt32(data);
        string fileName = Encoding.UTF8.GetString(data.Slice(4, len));
        return new FileRejectMessage(fileName);
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
