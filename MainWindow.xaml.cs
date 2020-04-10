using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CrosswordGenerator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Crossword crossword = new Crossword();
        
        public MainWindow()
        {
            InitializeComponent();
        }

        void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            crossword.ReadWords(WordsTextBox.Text);
            var result = crossword.Generate();
            sw.Stop();
            var generationTime = sw.Elapsed;

            sw.Restart();
            var image = ConvertToBitmapImage(Crossword.ToImage(result));
            sw.Stop();
            var drawingTime = sw.Elapsed;
            OutputImage.Source = image;

            MessageBox.Show($"generationTime: {generationTime}\ndrawingTime: {drawingTime}");
        }
        
        /// <summary>
        /// Takes a bitmap and converts it to an image that can be handled by WPF ImageBrush
        /// </summary>
        /// <param name="src">A bitmap image</param>
        /// <returns>The image as a BitmapImage for WPF</returns>
        public static BitmapImage ConvertToBitmapImage(Bitmap src)
        {
            var ms = new MemoryStream();
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            var image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
    }
}
