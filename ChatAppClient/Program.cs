using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Welcome to the Chat App!");

        Console.Write("Enter your name: ");
        string userName = Console.ReadLine();

        Console.Write("Enter the server address (e.g., 127.0.0.1:8888): ");
        string serverAddress = Console.ReadLine();

        if (string.IsNullOrEmpty(serverAddress))
        {
            serverAddress = "127.0.0.1:8888";
        }

        try
        {
            TcpClient client = new TcpClient();
            string[] addressParts = serverAddress.Split(':');
            string ipAddress = addressParts[0];
            int port = int.Parse(addressParts[1]);

            client.Connect(ipAddress, port);

            Console.WriteLine($"Connected to server at {serverAddress}...");

            Task.Run(() => ReceiveMessages(client));

            while (true)
            {
                Console.Write("Type a message (or 'exit' to end): ");
                string message = Console.ReadLine();

                if (message.ToLower() == "exit")
                {
                    break;
                }

                SendMessage(client, $"{userName}: {message}");
            }

            client.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static async Task ReceiveMessages(TcpClient client)
    {
        try
        {
            NetworkStream stream = client.GetStream();

            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead <= 0)
                {
                    break;
                }

                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine(receivedMessage);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving messages: {ex.Message}");
        }
    }

    static void SendMessage(TcpClient client, string message)
    {
        try
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }
}