using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CrosswordGenerator;
using Newtonsoft.Json;

namespace CrosswordApp
{
    /// <summary>
    /// Логика взаимодействия для CrosswordPage.xaml
    /// </summary>
    public partial class CrosswordPage : UserControl
    {
        CrosswordGenerator crosswordGenerator;

        Crossword crossword;

        public CrosswordPage(List<(string word, string definition)> words, string name)
        {
            InitializeComponent();

            for (var i = 0; i < words.Count; ++i)
                words[i] = (words[i].word.ToLower().Trim(), words[i].definition);

            crosswordGenerator = new CrosswordGenerator
            {
                words = words,
                name = name,
            };

            Generate();
        }

        void Generate()
        {
            crossword = crosswordGenerator.Generate();
            
            crossword.placements.Sort((v1, v2) => v2.index > v1.index ? -1 : 1);

            FillGrid(24);

            var unusedWords = crosswordGenerator.words.Count - crossword.placements.Count;
            UnusedWordsTextBox.Text = unusedWords.ToString();

            // OutputTextBox.Text = crossword.GetDefinitionsString();
        }

        void FillGrid(int cellSize)
        {
            var size = crossword.Size;

            CrosswordGrid.Children.Clear();
            CrosswordGrid.RowDefinitions.Clear();
            for (var i = 0; i < size.y; ++i)
                CrosswordGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(cellSize) });
            CrosswordGrid.ColumnDefinitions.Clear();
            for (var i = 0; i < size.x; ++i)
                CrosswordGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(cellSize) });

            var cells = new bool[size.x, size.y];
            foreach (var placement in crossword.placements)
            {
                for (var i = 0; i < placement.Width; ++i)
                {
                    for (var j = 0; j < placement.Height; ++j)
                    {
                        (int x, int y) pos = (placement.x + i, placement.y + j);
                        if (cells[pos.x, pos.y])
                            continue;

                        var c = new Border
                        {
                            Background = new SolidColorBrush(Color.FromRgb(44, 44, 44)),
                            BorderThickness = new Thickness(0.5),
                            BorderBrush = Brushes.White,
                        };
                        c.SetValue(Grid.RowProperty, pos.y);
                        c.SetValue(Grid.ColumnProperty, pos.x);
                        CrosswordGrid.Children.Add(c);

                        var a = new TextBlock
                        {
                            Text = crossword.words[placement.wordIndex].word[placement.isVertical ? j : i].ToString(),
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Visibility = ShowLettersCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Hidden,
                            Foreground = Brushes.White,
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
                    FontSize = 7,
                    Foreground = Brushes.White,
                };
                b.SetValue(Grid.RowProperty, placement.y);
                b.SetValue(Grid.ColumnProperty, placement.x);
                CrosswordGrid.Children.Add(b);
            }
        }

        void BackButton_Click(object sender, RoutedEventArgs e)
        {
            (Parent as MainCrosswordGenerationPage).ToWordsPage();
        }
        
        void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            Generate();
        }

        void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new SaveFileDialog
            {
                FileName = "crossword",
                Filter = "JSON file|*.json"
            };
            if (fileDialog.ShowDialog() != true)
                return;
            
            var jsonCrossword = JsonConvert.SerializeObject(crossword);
            File.WriteAllText(fileDialog.FileName, jsonCrossword);
        }
        
        void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new SaveFileDialog
            {
                FileName = "crossword",
                Filter = "Bitmap Image (.bmp)|*.bmp|Gif Image (.gif)|*.gif|JPEG Image (.jpeg)|*.jpeg|Png Image (.png)|*.png|Tiff Image (.tiff)|*.tiff|Wmf Image (.wmf)|*.wmf"
            };
            if (fileDialog.ShowDialog() != true)
                return;

            var filePath = fileDialog.FileName;
            var encoder = new JpegBitmapEncoder();

            var image = crossword.ToImage(ShowLettersOnImageCheckBox.IsChecked == true);
            var bitmapImage = Crossword.ConvertToBitmapImage(image);

            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            using (var filestream = new FileStream(filePath, FileMode.Create))
                encoder.Save(filestream);
        }

        void ShowLettersCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var tb in CrosswordGrid.Children.OfType<TextBlock>().Where(v => v.FontSize > 8))
            {
                tb.Visibility = Visibility.Visible;
            }
        }

        void ShowLettersCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var tb in CrosswordGrid.Children.OfType<TextBlock>().Where(v => v.FontSize > 8))
            {
                tb.Visibility = Visibility.Hidden;
            }
        }

        void FinishButton_OnClick(object sender, RoutedEventArgs e)
        {
            var messageBoxResult = System.Windows.MessageBox.Show(
                "Вы уверены, что хотите вернуться в меню? Все несохраненные изменения пропадут.", "Выход в меню",
                System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult != MessageBoxResult.Yes)
                return;
            
            (Parent as MainCrosswordGenerationPage).ToMenuPage();
        }
    }
}
