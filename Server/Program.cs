using Server;

ChatServer server = new ChatServer("127.0.0.1", 8080);
server.Start();

Console.WriteLine("Enter to stop the server.");
Console.ReadLine();
server.Stop();
