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

namespace Client_WPF_image
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var imageBytes = File.ReadAllBytes(openFileDialog.FileName);
                SendImageToServer(imageBytes);
            }
        }

        // Invio immagine da file
        private void SendImageToServer(byte[] imageBytes)
        {
            try
            {
                _tcpClient = new TcpClient("127.0.0.1", 12345);
                _networkStream = _tcpClient.GetStream();
                BinaryWriter writer = new BinaryWriter(_networkStream);

                writer.Write(imageBytes.Length); // Header con dimensione
                writer.Write(imageBytes); // Payload

                StatusLabel.Content = "Immagine inviata al server.";
                _tcpClient.Close();
            }
            catch (Exception ex)
            {
                StatusLabel.Content = $"Errore: {ex.Message}";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DrawingBoard drawingBoard = new DrawingBoard();
            drawingBoard.Show();
            this.Close();
        }

    }
}