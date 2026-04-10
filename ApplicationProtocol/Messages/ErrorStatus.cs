using System.Text;

namespace Application;

public class ErrorStatus : Message
{
    public string ErrorMessage { get; set; }

    public ErrorStatus(string errorMessage) : base(Tag.ErrorStatus)
        => ErrorMessage = errorMessage;

    public static ErrorStatus Deserialize(ReadOnlySpan<byte> data)
    {
        int length = BitConverter.ToInt32(data);
        string message = Encoding.UTF8.GetString(data.Slice(4, length));
        return new ErrorStatus(message);
    }

    public override byte[] SerializePayload()
    {
        byte[] msgBytes = Encoding.UTF8.GetBytes(ErrorMessage);
        List<byte> bytes =
        [
            ..BitConverter.GetBytes(msgBytes.Length),
            ..msgBytes
        ];
        return bytes.ToArray();
    }
}
