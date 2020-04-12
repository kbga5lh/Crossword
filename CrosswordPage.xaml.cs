using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace CrosswordApp
{
    /// <summary>
    /// Логика взаимодействия для CrosswordPage.xaml
    /// </summary>
    public partial class CrosswordPage : Page
    {
        List<(string word, string definition)> words;

        CrosswordGenerator crosswordGenerator;

        public CrosswordPage(List<(string word, string definition)> words)
        {
            InitializeComponent();

            this.words = words;
            for (int i = 0; i < this.words.Count; ++i)
                this.words[i] = (this.words[i].word.ToLower(), this.words[i].definition);

            crosswordGenerator = new CrosswordGenerator
            {
                Words = this.words,
            };

            Generate();
        }

        void Generate()
        {
            var sw = new Stopwatch();
            sw.Start();
            var crossword = crosswordGenerator.Generate();
            sw.Stop();
            var generationTime = sw.Elapsed;

            sw.Restart();
            var image = crossword.ToImage();
            sw.Stop();
            var drawingTime = sw.Elapsed;
            OutputImage.Source = Crossword.ConvertToBitmapImage(image);
            OutputImage.Width = image.Width / 2;
            OutputImage.Height = image.Height / 2;

            MessageBox.Show($"generationTime: {generationTime}\ndrawingTime: {drawingTime}");

            OutputTextBox.Text = crossword.GetDefinitionsString();

            File.WriteAllText("crossword.json", Newtonsoft.Json.JsonConvert.SerializeObject(crossword,
                new Newtonsoft.Json.JsonSerializerSettings() { }));
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            Generate();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var a = new SaveFileDialog();
            a.Filter = "Bitmap Image (.bmp)|*.bmp|Gif Image (.gif)|*.gif|JPEG Image (.jpeg)|*.jpeg|Png Image (.png)|*.png|Tiff Image (.tiff)|*.tiff|Wmf Image (.wmf)|*.wmf";
            if (a.ShowDialog() != true)
                return;

            string filePath = a.FileName;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create((BitmapImage)OutputImage.Source));

            using (var filestream = new FileStream(filePath, FileMode.Create))
                encoder.Save(filestream);
        }
    }
}
