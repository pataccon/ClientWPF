using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        int port = 12345; // Numero della porta del server

        // Crea un TcpListener
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Server started on port {port}. Waiting for a connection...");

        while (true)
        {
            // Accetta la conessione del Client
            var client = await listener.AcceptTcpClientAsync();
            Console.WriteLine("Client connected.");

            // Ricezione del Stream del Client
            var stream = client.GetStream();

            // Buffer per salvarsi i dati
            byte[] buffer = new byte[1024];

            // Leggi i dati dallo stream
            int bytesReceived = await stream.ReadAsync(buffer, 0, buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
            Console.WriteLine($"Received message: {message}");

            // Messaggio da mandare al client
            Console.WriteLine("Enter the message to response to client:");
            string response = Console.ReadLine()!;
            byte[] responseBuffer = Encoding.UTF8.GetBytes(response);

            // Sppedisci il messagio al client
            await stream.WriteAsync(responseBuffer, 0, responseBuffer.Length);
            Console.WriteLine("Message sent to the client.");

            // Chiudi la connessione con il Client
            client.Close();
            Console.WriteLine("Client disconnected.");
        }
    }
}