namespace Server.Messages;

public enum Tag : byte
{
    JoinRoom      = 1,
    ChatMessage   = 2,
    ChangeRoom    = 3,
    FileOffer     = 4,
    FileAccept    = 5,
    FileReject    = 6,
    FileData      = 7,
    OKStatus      = 8,
    ErrorStatus   = 9,
    FileRequest   = 10,
    SystemMessage = 11
}
