using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace Server_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpListener _tcpListener;
        private Thread _listenerThread; 
        private TcpClient _client;
        private NetworkStream _networkStream;
        private readonly int _port = 5000;

        public MainWindow()
        {
            InitializeComponent();
            StartServer();
        }

        // Avvio del server
        private void StartServer()
        {
            _tcpListener = new TcpListener(IPAddress.Any, _port);
            _tcpListener.Start();
            _listenerThread = new Thread(ListenForClients); // Thread principale
            _listenerThread.Start();
        }

        // Ascolto e accettazione della connessioni
        private void ListenForClients()
        {
            while (true)
            {
                _client = _tcpListener.AcceptTcpClient();
                _networkStream = _client.GetStream();
                Dispatcher.Invoke(() => ChatBox.AppendText("Client connected.\n"));

                Thread clientThread = new Thread(HandleClientComm);
                clientThread.Start(_client);
            }
        }

        // Gestione comunicazione client
        private void HandleClientComm(object clientObj)
        {
            TcpClient tcpClient = (TcpClient)clientObj;
            NetworkStream networkStream = tcpClient.GetStream();
            BinaryReader reader = new BinaryReader(networkStream);
            byte[] buffer = new byte[1024];
            int bytesRead;
            bool flag=false;
            while ((bytesRead = networkStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                if (message == ".You sent an image")
                {
                    Dispatcher.Invoke(() => ChatBox.AppendText("Client sent an image\n"));
                    flag = true;
                }
                else 
                {
                    Dispatcher.Invoke(() => ChatBox.AppendText("Client: " + message + "\n"));
                }
                if (flag) 
                {
                    int imageSize = reader.ReadInt32();
                    byte[] imageBytes = reader.ReadBytes(imageSize);
                    string imageName = "";
                    string stringData = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Minute;
                    
                    imageName = "image_" + stringData + ".png";
 
                    File.WriteAllBytes(imageName, imageBytes);
                }
                // Risposta al client
                //byte[] response = Encoding.ASCII.GetBytes("Server: " + message);
                //networkStream.Write(response, 0, response.Length);
            }
        }

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_networkStream != null && _client.Connected)
            {
                string message = ServerMessageTextBox.Text;
                byte[] data = Encoding.ASCII.GetBytes(message);
                _networkStream.Write(data, 0, data.Length);
                Dispatcher.Invoke(() => ChatBox.AppendText("Server: " + message + "\n"));
                ServerMessageTextBox.Clear();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _tcpListener.Stop();
            _listenerThread.Interrupt();
            _client?.Close();
        }
    }
}
//codice per ricevere l'immagine
/*

            int imageSize = reader.ReadInt32();
            byte[] imageBytes = reader.ReadBytes(imageSize)

            string recivedImageName = "received_image_" + new Random().Next(0,1000) + ".png";
            string imageFileName = Path.Combine(_cacheDirectory, recivedImageName);
            File.WriteAllBytes(imageFileName, imageBytes);
 */