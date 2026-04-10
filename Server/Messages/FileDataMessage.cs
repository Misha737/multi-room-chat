using System.Text;

namespace Server.Messages;

public class FileDataMessage : Message
{
    public string FileName { get; set; }
    public byte[] Data { get; set; }

    public FileDataMessage(string fileName, byte[] data) : base(Tag.FileData)
    {
        FileName = fileName;
        Data = data;
    }

    public static FileDataMessage Deserialize(ReadOnlySpan<byte> data)
    {
        int fileNameLen = BitConverter.ToInt32(data);
        string fileName = Encoding.UTF8.GetString(data.Slice(4, fileNameLen));
        int offset = 4 + fileNameLen;
        int dataLen = BitConverter.ToInt32(data.Slice(offset));
        byte[] fileData = data.Slice(offset + 4, dataLen).ToArray();
        return new FileDataMessage(fileName, fileData);
    }

    public override byte[] SerializePayload()
    {
        byte[] fileNameBytes = Encoding.UTF8.GetBytes(FileName);
        List<byte> bytes =
        [
            ..BitConverter.GetBytes(fileNameBytes.Length),
            ..fileNameBytes,
            ..BitConverter.GetBytes(Data.Length),
            ..Data
        ];
        return bytes.ToArray();
    }
}
