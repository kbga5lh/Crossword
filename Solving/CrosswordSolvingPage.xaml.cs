using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для CrosswordSolvingPage.xaml
    /// </summary>
    public partial class CrosswordSolvingPage : Page
    {
        Crossword crossword;

        public CrosswordSolvingPage(Crossword crossword)
        {
            InitializeComponent();

            this.crossword = crossword;

            FillGrid(24);

            DefinitionsTextBlock.Text = this.crossword.GetDefinitionsString();
        }

        void FillGrid(int cellSize)
        {
            var size = crossword.Size;

            CrosswordGrid.Children.Clear();
            CrosswordGrid.RowDefinitions.Clear();
            for (var i = 0; i < size.y; ++i)
                CrosswordGrid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(cellSize)});
            CrosswordGrid.ColumnDefinitions.Clear();
            for (var i = 0; i < size.x; ++i)
                CrosswordGrid.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(cellSize)});

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

                        var a = new TextBox
                        {
                            VerticalContentAlignment = VerticalAlignment.Center,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            Style = Resources["CrosswordLetterTextBox"] as Style,
                        };
                        a.GotFocus += (sender, args) =>
                        {
                            if (!(sender is TextBox tb)) return;
                            tb.SelectAll();
                        };
                        a.PreviewMouseLeftButtonDown += (sender, args) =>
                        {
                            if (!(sender is TextBox tb)) return;
                            if (tb.IsKeyboardFocusWithin) return;
                            args.Handled = true;
                            tb.Focus();
                        };
                        a.MaxLength = 1;

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
                    IsHitTestVisible = false,
                };
                b.SetValue(Grid.RowProperty, placement.y);
                b.SetValue(Grid.ColumnProperty, placement.x);
                CrosswordGrid.Children.Add(b);
            }
        }

        void BackButton_Click(object sender, RoutedEventArgs e)
        {
            (Parent as MainWindow).Content = new MainMenuPage();
        }

        void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            var solvedWords = SolvedWords;
            if (solvedWords == crossword.words.Count)
            {
                MessageBox.Show("Вы правильно решили кроссворд!");
            }
            else
            {
                MessageBox.Show($"К сожалению, Вы отгадали только {solvedWords} из {crossword.words.Count} слов. Попробуйте еще раз.");
            }
        }

        int SolvedWords
        {
            get
            {
                var result = 0;
                var size = crossword.Size;

                var cells = new char[size.x, size.y];
                foreach (var letterTextBox in CrosswordGrid.Children)
                {
                    if (!(letterTextBox is TextBox tb))
                        continue;
                    var x = (int) tb.GetValue(Grid.ColumnProperty);
                    var y = (int) tb.GetValue(Grid.RowProperty);
                    if (tb.Text.Length < 1)
                        continue;
                    cells[x, y] = tb.Text.ToLower()[0];
                }

                foreach (var placement in crossword.placements)
                {
                    var solvedCorrectly = true;
                    for (var i = 0; i < placement.Width && solvedCorrectly; ++i)
                    {
                        for (var j = 0; j < placement.Height && solvedCorrectly; ++j)
                        {
                            (int x, int y) pos = (placement.x + i, placement.y + j);
                            if (cells[pos.x, pos.y] !=
                                crossword.words[placement.wordIndex].word[placement.isVertical ? j : i])
                            {
                                solvedCorrectly = false;
                            }
                        }
                    }
                    if (solvedCorrectly)
                        result++;
                }

                return result;
            }
        }
    }
}