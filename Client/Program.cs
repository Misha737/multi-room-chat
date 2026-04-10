using Client;

Console.WriteLine("=== Chat Client ===");

Console.Write("Server host [127.0.0.1]: ");
string host = Console.ReadLine()?.Trim() ?? "";
if (string.IsNullOrEmpty(host)) host = "127.0.0.1";

Console.Write("Server port [8080]: ");
string portStr = Console.ReadLine()?.Trim() ?? "";
int port = string.IsNullOrEmpty(portStr) ? 8080 : int.Parse(portStr);

Console.Write("Your name: ");
string name = Console.ReadLine()?.Trim() ?? "Anonymous";
if (string.IsNullOrEmpty(name)) name = "Anonymous";

Console.Write("Room ID (1 or 2): ");
string roomStr = Console.ReadLine()?.Trim() ?? "1";
int roomId = string.IsNullOrEmpty(roomStr) ? 1 : int.Parse(roomStr);

try
{
    ChatClient client = new ChatClient(host, port, name, roomId);
    client.Start();
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] Could not connect: {ex.Message}");
}
