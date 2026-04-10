using Server.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server;

public class Room
{
    public int Id { get; init; }
    public string Name { get; init; }
    private Queue<Message> messages = new();
    public object _lock { get; init; } = new();
    public Room(int id, string name)
    {
        Id = id;
        Name = name;
    }
}