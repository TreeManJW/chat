using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    // Add this line to store connected clients
    static List<TcpClient> connectedClients = new List<TcpClient>();

    static async Task Main(string[] args)
    {
        TcpListener server = new TcpListener(IPAddress.Any, 8888);
        server.Start();
        Console.WriteLine("Chat server started...");

        while (true)
        {
            TcpClient client = await server.AcceptTcpClientAsync();
            _ = HandleClientAsync(client);
        }
    }

    static async Task HandleClientAsync(TcpClient client)
    {
        Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");

        // Add the connected client to the list
        connectedClients.Add(client);

        NetworkStream stream = client.GetStream();

        while (true)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            if (bytesRead <= 0)
            {
                break; // Client disconnected
            }

            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Received from {client.Client.RemoteEndPoint}: {message}");

            // Broadcast the message to all clients
            await BroadcastMessageAsync(message, client);
        }

        Console.WriteLine($"Client disconnected: {client.Client.RemoteEndPoint}");

        // Remove the disconnected client from the list
        connectedClients.Remove(client);

        client.Close();
    }

    static async Task BroadcastMessageAsync(string message, TcpClient senderClient)
    {
        foreach (var client in connectedClients)
        {
            if (client != senderClient)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(buffer, 0, buffer.Length);
                await stream.FlushAsync();
            }
        }
    }
}