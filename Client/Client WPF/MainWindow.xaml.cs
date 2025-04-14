using System;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;
using System.ComponentModel;
using System.Threading;
using System.Net;
//using static System.Net.WebRequestMethods;


namespace Client_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private Thread _listenerThread;
        private BinaryWriter _writer;

        public MainWindow()
        {
            InitializeComponent();
        }

        // Connessione al server
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            string ip = txtInputip.Text;
            int port = 5000;

            try
            {
                _tcpClient = new TcpClient(ip, port);
                _networkStream = _tcpClient.GetStream();
                _listenerThread = new Thread(ListenForMessages); // Thread per ricezione asincrona
                _listenerThread.Start();
                _writer = new BinaryWriter(_networkStream);


                btnConnect.IsEnabled = false;
                btnDisconnect.IsEnabled = true;

                ChatBox.AppendText("Connection Successful to server: " + ip + "\n");
            }
            catch (Exception ex)
            {
                ChatBox.AppendText("Error connecting to server: " + ex.Message + "\n");
            }
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            Disconect();
        }

        private void Disconect()
        {
            _networkStream.Close();
            _tcpClient.Close();
            _listenerThread.Interrupt();

            btnConnect.IsEnabled = true;
            btnDisconnect.IsEnabled = false;

            ChatBox.AppendText("Disconnected from server.\n");
        }

        // Invio messaggi
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text;
            if (!string.IsNullOrEmpty(message))
            {
                byte[] data = Encoding.ASCII.GetBytes(message);
                Send(data);
                ChatBox.AppendText("You: " + message + "\n");

                MessageTextBox.Clear();
            }
        }

        // Ricezione messaggi in background
        private void ListenForMessages()
        {
            byte[] buffer = new byte[1024];
            int bytesRead;

            while (true)
            {
                try
                {
                    bytesRead = _networkStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Dispatcher.Invoke(() => ChatBox.AppendText("Server: " + message + "\n"));
                }
                catch
                {
                    break;
                }
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*webp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var imageBytes = File.ReadAllBytes(openFileDialog.FileName);
                string notif = ".You sent an image";
                ChatBox.AppendText(notif.Trim('.')+"\n");
                byte[] notifBytes = Encoding.ASCII.GetBytes(notif);
                Send(notifBytes);

                _writer.Write(imageBytes.Length);
                _writer.Write(imageBytes);

            }
        }

        public void Send(byte[] data) 
        {
            _networkStream.Write(data, 0, data.Length);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_networkStream != null)
                _networkStream.Close();
            if (_tcpClient != null)
                _tcpClient.Close();
            if (_listenerThread != null && _listenerThread.IsAlive)
                _listenerThread.Interrupt();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Disconect();
            Application.Current.Shutdown();
        }

    }
}