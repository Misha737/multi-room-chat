using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Messages;

public abstract class Message
{
    public Message()
    {
    }

    public abstract byte[] Serialize();
}
