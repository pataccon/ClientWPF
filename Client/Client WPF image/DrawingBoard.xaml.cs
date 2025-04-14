using System;
using System.Collections.Generic;
using System.IO;
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
    /// Logica di interazione per DrawingBoard.xaml
    /// </summary>
    public partial class DrawingBoard : Window
    {
        private TcpClient client;
        private NetworkStream stream;
        private Point _lastPoint;

        public DrawingBoard()
        {
            InitializeComponent();
        }
        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _lastPoint = e.GetPosition(DrawingCanvas);
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPoint = e.GetPosition(DrawingCanvas);
                var line = new Line
                {
                    X1 = _lastPoint.X,
                    Y1 = _lastPoint.Y,
                    X2 = currentPoint.X,
                    Y2 = currentPoint.Y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };
                DrawingCanvas.Children.Add(line);
                _lastPoint = currentPoint;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SendImageToServer(SaveCanvasToImage(DrawingCanvas, "name"));
        }

        private void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Handle
        }

        // Conversione canvas in PNG
        private byte[] SaveCanvasToImage(Canvas canvas, string filename)
        {
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(canvas);
                context.DrawRectangle(vb, null, new Rect(new Point(), new Size(canvas.ActualWidth, canvas.ActualHeight)));
            }

            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);

            //Trasformo l'immagine in un array di byte
            using (MemoryStream ms = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(ms);
                return ms.ToArray();
            }
        }

        private void SendImageToServer(byte[] imageBytes)
        {
            try
            {
                client = new TcpClient("127.0.0.1", 12345);
                stream = client.GetStream();
                BinaryWriter writer = new BinaryWriter(stream);

                writer.Write(imageBytes.Length);
                writer.Write(imageBytes);

                MessageBox.Show("Immagine inviata al server.");
                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore: {ex.Message}");
            }
        }
    }
}
