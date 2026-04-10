using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Messages;

public abstract class Message
{
    public Tag Tag { get; init; }
    public Message(Tag tag)
    {
        Tag = tag;
    }

    public byte[] Serialize()
    {
        byte[] payload = SerializePayload();
        int length = payload.Length;
        return [
            (byte)Tag,
            .. BitConverter.GetBytes(length),
            ..payload];
    }

    public abstract byte[] SerializePayload();
}
