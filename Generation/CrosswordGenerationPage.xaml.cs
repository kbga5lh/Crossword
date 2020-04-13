using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CrosswordApp
{
    /// <summary>
    /// Логика взаимодействия для CrosswordPage.xaml
    /// </summary>
    public partial class CrosswordPage : Page
    {
        CrosswordGenerator crosswordGenerator;

        Crossword crossword;

        public CrosswordPage(List<(string word, string definition)> words)
        {
            InitializeComponent();

            for (int i = 0; i < words.Count; ++i)
                words[i] = (words[i].word.ToLower().Trim(), words[i].definition);

            crosswordGenerator = new CrosswordGenerator
            {
                Words = words,
            };

            Generate();
        }

        void Generate()
        {
            var sw = new Stopwatch();
            sw.Start();
            crossword = crosswordGenerator.Generate();
            sw.Stop();
            var generationTime = sw.Elapsed;

            crossword.placements.Sort((v1, v2) => v2.index > v1.index ? -1 : 1);

            FillGrid();

            // OutputTextBox.Text = crossword.GetDefinitionsString();

            File.WriteAllText("crossword.json", Newtonsoft.Json.JsonConvert.SerializeObject(crossword,
                new Newtonsoft.Json.JsonSerializerSettings() { }));
        }

        void FillGrid(int cellSize = 24)
        {
            var size = crossword.Size;

            CrosswordGrid.Children.Clear();
            CrosswordGrid.RowDefinitions.Clear();
            for (int i = 0; i < size.y; ++i)
                CrosswordGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(cellSize) });
            CrosswordGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < size.x; ++i)
                CrosswordGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(cellSize) });

            var cells = new bool[size.x, size.y];
            foreach (var placement in crossword.placements)
            {
                for (int i = 0; i < placement.Width; ++i)
                {
                    for (int j = 0; j < placement.Height; ++j)
                    {
                        (int x, int y) pos = (placement.x + i, placement.y + j);
                        if (cells[pos.x, pos.y] == true)
                            continue;

                        var c = new Border
                        {
                            Background = Brushes.White,
                            BorderThickness = new Thickness(0.5),
                            BorderBrush = Brushes.Black,
                        };
                        c.SetValue(Grid.RowProperty, pos.y);
                        c.SetValue(Grid.ColumnProperty, pos.x);
                        CrosswordGrid.Children.Add(c);

                        var a = new TextBlock
                        {
                            Text = crossword.words[placement.wordIndex].word[placement.isVertical ? j : i].ToString(),
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Visibility = ShowLettersCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Hidden
                        };
                        a.SetValue(Grid.RowProperty, pos.y);
                        a.SetValue(Grid.ColumnProperty, pos.x);
                        CrosswordGrid.Children.Add(a);

                        cells[pos.x, pos.y] = true;
                    }
                }

                var b = new TextBlock
                {
                    Text = placement.index.ToString(),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(2, 2, 0, 0),
                    FontSize = 7
                };
                b.SetValue(Grid.RowProperty, placement.y);
                b.SetValue(Grid.ColumnProperty, placement.x);
                CrosswordGrid.Children.Add(b);
            }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            Generate();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var a = new SaveFileDialog
            {
                FileName = "crossword",
                Filter = "Bitmap Image (.bmp)|*.bmp|Gif Image (.gif)|*.gif|JPEG Image (.jpeg)|*.jpeg|Png Image (.png)|*.png|Tiff Image (.tiff)|*.tiff|Wmf Image (.wmf)|*.wmf"
            };
            if (a.ShowDialog() != true)
                return;

            string filePath = a.FileName;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();

            var image = crossword.ToImage();
            var bitmapImage = Crossword.ConvertToBitmapImage(image);

            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            using (var filestream = new FileStream(filePath, FileMode.Create))
                encoder.Save(filestream);
        }

        private void ShowLettersCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var tb in CrosswordGrid.Children.OfType<TextBlock>().Where(v => v.FontSize > 8))
            {
                tb.Visibility = Visibility.Visible;
            }
        }

        private void ShowLettersCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var tb in CrosswordGrid.Children.OfType<TextBlock>().Where(v => v.FontSize > 8))
            {
                tb.Visibility = Visibility.Hidden;
            }
        }
    }
}
