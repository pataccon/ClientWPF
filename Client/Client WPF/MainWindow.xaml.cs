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
using static System.Net.Mime.MediaTypeNames;

namespace Client_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private Thread _listenThread;
        private BinaryWriter _writer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            string ip = txtInputip.Text;
            int port = 5000;

            try
            {
                _tcpClient = new TcpClient(ip, port);
                _networkStream = _tcpClient.GetStream();
                _writer = new BinaryWriter(_networkStream);
                _listenThread = new Thread(ListenMsg);
                _listenThread.Start();


                btnConnect.IsEnabled = false;
                btnConnect.Visibility = Visibility.Hidden;

                btnDisconnect.IsEnabled = true;
                btnDisconnect.Visibility = Visibility.Visible;

                btnSend.IsEnabled = true;

                btnSendFile.IsEnabled = true;

                txtOutputChat.AppendText($"Connected successfully to the server:  {ip }\n");
            }
            catch (Exception ex)
            {
                txtOutputChat.AppendText($"Error while trying to connect to the server: {ex.Message}\n");
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
            _listenThread.Interrupt();

            btnConnect.IsEnabled = true;
            btnConnect.Visibility = Visibility.Visible;

            btnDisconnect.IsEnabled = false;
            btnDisconnect.Visibility = Visibility.Hidden;

            btnSend.IsEnabled = false;

            btnSendFile.IsEnabled = false;

            txtOutputChat.AppendText("Disconnected from the server.\n");
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            string pureMessage=txtInputMessage.Text;
            string message = ": " + pureMessage;
            if (!string.IsNullOrEmpty(pureMessage))
            {
                byte[] data = Encoding.ASCII.GetBytes(message);
                Send(data);
                txtOutputChat.AppendText("You" + message + "\n");

                txtInputMessage.Clear();
            }
        }

        private void ListenMsg()
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
                    Dispatcher.Invoke(() => txtOutputChat.AppendText("Server: " + message + "\n"));
                }catch
                {
                    break;
                }
            }
        }

        private void btnSendFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = ""
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string name=openFileDialog.FileName;
                var imageBytes = File.ReadAllBytes(name);
                string extension = name.Split('.')[name.Split('.').Length-1];
                string notif =$"You sent a file <.{extension}>";
                txtOutputChat.AppendText(notif+"\n");
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

    }
}