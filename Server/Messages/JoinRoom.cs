using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Messages
{
    public class JoinRoom : Message
    {
        public ClientInfo ClientInfo { get; set; }
        private JoinRoom(ClientInfo clientInfo) : base()
        {
            ClientInfo = clientInfo;
        }

        public static JoinRoom Deserialize(ReadOnlySpan<byte> data)
        {
                ClientInfo clientInfo = ClientInfo.Deserialize(data);
                return new JoinRoom(clientInfo);
        }

        public override byte[] Serialize()
        {
            byte tag = (byte)Tag.JoinRoom;
            byte[] data = ClientInfo.Serialize();
            int length = data.Length;
            return [tag, .. BitConverter.GetBytes(length), ..data];
        }
    }
}
