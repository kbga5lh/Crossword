using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Логика взаимодействия для CrosswordPage.xaml
    /// </summary>
    public partial class CrosswordPage : Page
    {
        List<(string word, string definition)> words;

        Crossword crosswordGenerator;

        public CrosswordPage(List<(string word, string definition)> words)
        {
            InitializeComponent();

            this.words = words;
            for (int i = 0; i < this.words.Count; ++i)
                this.words[i] = (this.words[i].word.ToLower(), this.words[i].definition);

            crosswordGenerator = new Crossword
            {
                Words = this.words.ConvertAll(v => v.word)
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
            var image = Crossword.ToImage(crossword);
            sw.Stop();
            var drawingTime = sw.Elapsed;
            OutputImage.Source = Crossword.ConvertToBitmapImage(image);
            OutputImage.Width = image.Width / 2;
            OutputImage.Height = image.Height / 2;

            MessageBox.Show($"generationTime: {generationTime}\ndrawingTime: {drawingTime}");
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            Generate();
        }
    }
}
