namespace Application;

public abstract class Message
{
    public Tag Tag { get; init; }

    protected Message(Tag tag) => Tag = tag;

    public byte[] Serialize()
    {
        byte[] payload = SerializePayload();
        List<byte> bytes =
        [
            (byte)Tag,
            ..BitConverter.GetBytes(payload.Length),
            ..payload
        ];
        return bytes.ToArray();
    }

    public abstract byte[] SerializePayload();
}
