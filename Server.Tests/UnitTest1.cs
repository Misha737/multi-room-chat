using Xunit;
using Server;
using Server.Messages;
using System.Text;

namespace Server.Tests;

public class UnitTest1
{
    [Fact]
    public void ClientInfoDeserialisation_RightRawBytes_ClientInfoIsCorrect()
    {
        int Id = 555;
        string Name = "TestName";
        int RoomId = 123;
        byte[] Bytes = [.. BitConverter.GetBytes(Id), .. BitConverter.GetBytes(RoomId), .. Encoding.UTF8.GetBytes(Name)];

        ClientInfo clientInfo = ClientInfo.Deserialize(Bytes);
        Assert.Equal(Id, clientInfo.Id);
        Assert.Equal(RoomId, clientInfo.RoomId);
        Assert.Equal(Name, clientInfo.Name);
    }

    [Fact]
    public void ClientInfoSerialisation_ClientInfo_RawBytesIsEqual()
    {
        int Id = 555;
        string Name = "TestName";
        int RoomId = 123;
        byte[] Bytes = [.. BitConverter.GetBytes(Id), .. BitConverter.GetBytes(RoomId), .. Encoding.UTF8.GetBytes(Name)];

        ClientInfo clientInfo = new ClientInfo(Id, RoomId, Name);
        byte[] actual = clientInfo.Serialize();

        Assert.Equal(Bytes, actual);
    }

    [Fact]
    public void JoinRoomDeserialisation_RightRawBytes_JoinRoomIsCorrect()
    {
        byte tag = (byte)Tag.JoinRoom;
        int Id = 555;
        string Name = "TestName";
        int RoomId = 123;
        byte[] Bytes = [tag, .. BitConverter.GetBytes(4 + Name.Length), .. BitConverter.GetBytes(Id), .. BitConverter.GetBytes(RoomId), .. Encoding.UTF8.GetBytes(Name)];

        JoinRoom joinRoom = JoinRoom.Deserialize(Bytes);

    }
}
