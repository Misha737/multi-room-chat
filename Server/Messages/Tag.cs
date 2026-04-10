using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Messages;

public enum Tag : byte
{
    JoinRoom = 1,
    ChatMessage = 2,
    ChangeRoom = 3,
    FileOffer = 4,
    FileAccept = 5,
    FileReject = 6,
    FileData = 7
}