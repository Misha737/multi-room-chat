using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Messages;

public class OKStatus : Message
{
    public OKStatus() : base(Tag.OKStatus)
    {
    }

    public static OKStatus Deserialize()
    {
        return new OKStatus();
    }

    public override byte[] SerializePayload()
    {
        return Array.Empty<byte>();
    }
}

