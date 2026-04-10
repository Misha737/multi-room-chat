namespace Server.Messages;

public class OKStatus : Message
{
    public OKStatus() : base(Tag.OKStatus) { }

    public static OKStatus Deserialize() => new();

    public override byte[] SerializePayload() => Array.Empty<byte>();
}
