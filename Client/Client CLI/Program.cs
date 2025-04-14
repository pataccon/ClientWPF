using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string serverIp = "127.0.0.1"; // IP del server
        int port = 12345; // Porta TCP

        try
        {
            
            ConsoleKeyInfo key;
            do
            {
                var client = new TcpClient(); // Creazione socket TCP
                await client.ConnectAsync(serverIp, port); // Three-way handshake
                Console.WriteLine("Connected to the server.");

                var stream = client.GetStream(); // Ottenimento stream di rete

                // Invio messaggio
                Console.WriteLine("Enter the message to send to server:");
                string message = Console.ReadLine()!;
                byte[] buffer = Encoding.UTF8.GetBytes(message);

                // invio del messagio al server
                await stream.WriteAsync(buffer, 0, buffer.Length);
                Console.WriteLine("Message sent to the server.");

                // Buffer per salvare i dati 
                byte[] responseBuffer = new byte[1024];

                // Ricezione risposta
                int bytesReceived = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesReceived);
                Console.WriteLine($"Received message: {response}");

                
                Console.WriteLine("Enter [X] to disconect");
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.X)
                {
                    // Chiusura connessione
                    client.Close();
                    Console.WriteLine("Disconnected from the server.");
                    break;
                }
            } while (true);

            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}