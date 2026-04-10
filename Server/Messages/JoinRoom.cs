using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Messages;

public class JoinRoom : Message
{
    public ClientInfo ClientInfo { get; set; }
    private JoinRoom(ClientInfo clientInfo) : base(Tag.JoinRoom)
    {
        ClientInfo = clientInfo;
    }

    public static JoinRoom Deserialize(ReadOnlySpan<byte> data)
    {
        ClientInfo clientInfo = ClientInfo.Deserialize(data);
        return new JoinRoom(clientInfo);
    }

    public override byte[] SerializePayload()
    {
        byte[] clientInfoData = ClientInfo.Serialize();
        return clientInfoData;
    }
}