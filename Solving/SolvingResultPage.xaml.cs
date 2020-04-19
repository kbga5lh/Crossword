using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CrosswordApp
{
    public partial class SolvingResultPage : Page
    {
        Crossword crossword;
        readonly char[,] enteredLetters;

        public SolvingResultPage(Crossword crossword, char[,] enteredLetters, TimeSpan elapsedTime)
        {
            InitializeComponent();

            this.crossword = crossword;
            this.enteredLetters = enteredLetters;

            CrosswordNameTextBlock.Text = this.crossword.name;
            var solvedWords = SolvedWords;
            RightWordsTextBlock.Text =
                $"{solvedWords} из {crossword.words.Count} ({Math.Round(solvedWords / (double) crossword.words.Count * 100, 2)}%)";
            ElapsedTimeTextBlock.Text = elapsedTime.ToString("hh':'mm':'ss");

            FillGrid(24);
        }

        int SolvedWords
        {
            get
            {
                var result = 0;

                foreach (var placement in crossword.placements)
                {
                    var solvedCorrectly = true;
                    for (var i = 0; i < placement.Width && solvedCorrectly; ++i)
                    for (var j = 0; j < placement.Height && solvedCorrectly; ++j)
                    {
                        (int x, int y) pos = (placement.x + i, placement.y + j);
                        var c1 = enteredLetters[pos.x, pos.y];
                        var c2 = crossword.words[placement.wordIndex].word[placement.isVertical ? j : i];
                        if (c1 != Correct(c2)) solvedCorrectly = false;
                    }

                    if (solvedCorrectly)
                        result++;
                }

                return result;
            }
        }

        void FinishButton_OnClick(object sender, RoutedEventArgs e)
        {
            var messageBoxResult = MessageBox.Show(
                "Вы уверены, что хотите вернуться в меню?",
                "Выход в меню",
                MessageBoxButton.YesNo);
            if (messageBoxResult != MessageBoxResult.Yes)
                return;

            (Parent as MainWindow).Content = new MainMenuPage();
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

            var rightLetters = GetRightLetters();
            var cells = new bool[size.x, size.y];
            foreach (var placement in crossword.placements)
            {
                for (var i = 0; i < placement.Width; ++i)
                for (var j = 0; j < placement.Height; ++j)
                {
                    (int x, int y) pos = (placement.x + i, placement.y + j);
                    if (cells[pos.x, pos.y])
                        continue;

                    var isLetterCorrect = enteredLetters[pos.x, pos.y] == rightLetters[pos.x, pos.y];

                    var c = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(44, 44, 44)),
                        BorderThickness = new Thickness(0.5),
                        BorderBrush = Brushes.White
                    };
                    c.SetValue(Grid.RowProperty, pos.y);
                    c.SetValue(Grid.ColumnProperty, pos.x);
                    CrosswordGrid.Children.Add(c);

                    var a = new TextBlock
                    {
                        Text = crossword.words[placement.wordIndex].word[placement.isVertical ? j : i].ToString(),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = isLetterCorrect ? Brushes.White : new SolidColorBrush(Color.FromRgb(255, 60, 60)),
                        FontWeight = FontWeights.SemiBold
                    };
                    a.SetValue(Grid.RowProperty, pos.y);
                    a.SetValue(Grid.ColumnProperty, pos.x);
                    CrosswordGrid.Children.Add(a);

                    cells[pos.x, pos.y] = true;
                }

                var b = new TextBlock
                {
                    Text = placement.index.ToString(),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(2, 2, 0, 0),
                    FontSize = 7,
                    Foreground = Brushes.White
                };
                b.SetValue(Grid.RowProperty, placement.y);
                b.SetValue(Grid.ColumnProperty, placement.x);
                CrosswordGrid.Children.Add(b);
            }
        }

        char[,] GetRightLetters()
        {
            var size = crossword.Size;

            var cells = new char[size.x, size.y];
            foreach (var placement in crossword.placements)
                for (var i = 0; i < placement.Width; ++i)
                for (var j = 0; j < placement.Height; ++j)
                {
                    (int x, int y) pos = (placement.x + i, placement.y + j);
                    cells[pos.x, pos.y] = crossword.words[placement.wordIndex].word[placement.isVertical ? j : i];
                }

            return cells;
        }

        char Correct(char initial)
        {
            return initial == 'ё' ? 'е' : initial;
        }
    }
}