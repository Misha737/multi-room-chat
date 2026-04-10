namespace Server.Messages;

public class JoinRoom : Message
{
    public ClientInfo ClientInfo { get; set; }

    public JoinRoom(ClientInfo clientInfo) : base(Tag.JoinRoom)
        => ClientInfo = clientInfo;

    public static JoinRoom Deserialize(ReadOnlySpan<byte> data)
        => new(ClientInfo.Deserialize(data));

    public override byte[] SerializePayload()
        => ClientInfo.Serialize();
}
