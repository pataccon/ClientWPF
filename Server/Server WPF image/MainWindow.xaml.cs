using Microsoft.Win32;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Server_WPF_image
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpListener _tcpListener;
        private Thread _listenerThread;
        private string _cacheDirectory;
        private TcpClient _client;
        private NetworkStream _networkStream;
        private readonly int _port = 5000;

        public MainWindow()
        {
            InitializeComponent();
            SetCacheDirectory();
            StartServer();
        }

        // Imposta la cartella di cache nella directory dell'applicazione
        private void SetCacheDirectory()
        {
            // Ottieni il percorso dell'applicazione
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _cacheDirectory = Path.Combine(appDirectory, "Cache");

            // Crea la cartella Cache se non esiste
            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }
        }

        private void StartServer()
        {
            _listenerThread = new Thread(() =>
            {
                _tcpListener = new TcpListener(IPAddress.Any, 12345);
                _tcpListener.Start();
                Dispatcher.Invoke(() => StatusLabel.Content = "Server in ascolto...");

                while (true)
                {
                    var client = _tcpListener.AcceptTcpClient();
                    Dispatcher.Invoke(() => StatusLabel.Content = "Client connesso.");

                    ThreadPool.QueueUserWorkItem(HandleClient, client);
                }
            });

            _listenerThread.Start();
        }

        private void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream networkStream = client.GetStream();
            BinaryReader reader = new BinaryReader(networkStream);

            int imageSize = reader.ReadInt32(); // Lettura header
            byte[] imageBytes = reader.ReadBytes(imageSize);  // Lettura payload


            // Salva l'immagine nella cartella di cache
            string recivedImageName = "received_image_" + new Random().Next(0,1000) + ".png";
            string imageFileName = Path.Combine(_cacheDirectory, recivedImageName);
            File.WriteAllBytes(imageFileName, imageBytes);

            // Visualizzazione in UI
            Dispatcher.Invoke(() =>
            {
                StatusLabel.Content = "Immagine ricevuta e salvata nella cache.";
                DisplayImage(imageFileName); // Visualizza l'immagine dalla cache
            });

            client.Close();
        }

        private void DisplayImage(string imagePath)
        {
            // Carica l'immagine dalla cartella di cache
            var image = new System.Windows.Media.Imaging.BitmapImage(new Uri(imagePath));
            ReceivedImage.Source = image;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ReceivedImage.Source != null)
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string destinationPath = saveFileDialog.FileName;

                    // Sposta l'immagine dalla cache alla destinazione
                    string sourcePath = Path.Combine(_cacheDirectory, "received_image.png");
                    if (File.Exists(sourcePath))
                    {
                        File.Copy(sourcePath, destinationPath); // Sposta l'immagine
                        StatusLabel.Content = "Immagine salvata con successo.";
                    }
                    else
                    {
                        StatusLabel.Content = "Errore: Immagine non trovata.";
                    }
                }
            }
            else
            {
                StatusLabel.Content = "Nessuna immagine ricevuta.";
            }
        }
    }
}