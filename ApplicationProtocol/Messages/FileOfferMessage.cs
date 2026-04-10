using System.Text;

namespace Application;

public class FileOfferMessage : Message
{
    public string SenderName { get; set; }
    public string FileName { get; set; }
    public int FileSize { get; set; }

    public FileOfferMessage(string senderName, string fileName, int fileSize) : base(Tag.FileOffer)
    {
        SenderName = senderName;
        FileName = fileName;
        FileSize = fileSize;
    }

    public static FileOfferMessage Deserialize(ReadOnlySpan<byte> data)
    {
        int senderNameLen = BitConverter.ToInt32(data);
        string senderName = Encoding.UTF8.GetString(data.Slice(4, senderNameLen));
        int offset = 4 + senderNameLen;
        int fileNameLen = BitConverter.ToInt32(data.Slice(offset));
        string fileName = Encoding.UTF8.GetString(data.Slice(offset + 4, fileNameLen));
        offset += 4 + fileNameLen;
        int fileSize = BitConverter.ToInt32(data.Slice(offset));
        return new FileOfferMessage(senderName, fileName, fileSize);
    }

    public override byte[] SerializePayload()
    {
        byte[] senderNameBytes = Encoding.UTF8.GetBytes(SenderName);
        byte[] fileNameBytes = Encoding.UTF8.GetBytes(FileName);
        List<byte> bytes =
        [
            ..BitConverter.GetBytes(senderNameBytes.Length),
            ..senderNameBytes,
            ..BitConverter.GetBytes(fileNameBytes.Length),
            ..fileNameBytes,
            ..BitConverter.GetBytes(FileSize)
        ];
        return bytes.ToArray();
    }
}
