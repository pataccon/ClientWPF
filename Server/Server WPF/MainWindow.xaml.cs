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
        private readonly int _port = 5000;
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private Thread _waitThread; 

        public MainWindow()
        {
            InitializeComponent();
            StartServer();
        }

        private void StartServer()
        {
            _tcpListener = new TcpListener(IPAddress.Any, _port);
            _tcpListener.Start();
            _waitThread = new Thread(WaitForClients);
            _waitThread.Start();
        }

        private void WaitForClients()
        {
            while (true)
            {
                _tcpClient = _tcpListener.AcceptTcpClient();
                _networkStream = _tcpClient.GetStream();
                Dispatcher.Invoke(() => txtOutputChat.AppendText("A client has connected.\n"));

                Thread listenThread = new Thread(ListenMsg);
                listenThread.Start(_tcpClient);
            }
        }

        private void ListenMsg(object clientObj)
        {
            TcpClient tcpClient = (TcpClient)clientObj;
            NetworkStream networkStream = tcpClient.GetStream();
            BinaryReader reader = new BinaryReader(networkStream);
            byte[] buffer = new byte[1024];
            int bytesRead;
            bool flag=false;
            string extension = "";

            while ((bytesRead = networkStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                if (!(message.Contains( ':')))
                {
                    extension =message.Split('<')[1].Trim('>');
                    Dispatcher.Invoke(() => txtOutputChat.AppendText($"Client sent a file <{extension}>\n"));
                    flag = true;
                }
                else 
                {
                    Dispatcher.Invoke(() => txtOutputChat.AppendText("Client" + message + "\n"));
                }
                if (flag) 
                {
                    int fileSize = reader.ReadInt32();
                    byte[] fileBytes = reader.ReadBytes(fileSize);
                    string fileName = "";
                    string dateString = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "-" + DateTime.Now.Minute.ToString();
                    
                    fileName = "file_" + dateString+extension;
 
                    File.WriteAllBytes(fileName, fileBytes);
                }
                
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (_networkStream != null && _tcpClient.Connected)
            {
                string message = txtInputMsg.Text;
                byte[] data = Encoding.ASCII.GetBytes(message);
                _networkStream.Write(data, 0, data.Length);
                Dispatcher.Invoke(() => txtOutputChat.AppendText("You: " + message + "\n"));
                txtInputMsg.Clear();
            }
        }

    }
}