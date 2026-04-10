namespace Server.Messages;

public class ChangeRoomMessage : Message
{
    public int NewRoomId { get; set; }

    public ChangeRoomMessage(int newRoomId) : base(Tag.ChangeRoom)
        => NewRoomId = newRoomId;

    public static ChangeRoomMessage Deserialize(ReadOnlySpan<byte> data)
        => new(BitConverter.ToInt32(data));

    public override byte[] SerializePayload()
        => BitConverter.GetBytes(NewRoomId);
}
