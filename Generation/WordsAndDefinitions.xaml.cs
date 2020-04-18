using System;
using System.Windows;
using System.Windows.Controls;

namespace CrosswordApp
{
    public partial class WordsAndDefinitions : UserControl
    {
        public const int maxCount = 200;
        public int Count => WordsStackPanel.Children.Count;

        public event EventHandler CountChange;

        public WordsAndDefinitions()
        {
            InitializeComponent();

            AddWordAndDefinition();
        }

        public void AddWordAndDefinition()
        {
            if (Count >= maxCount)
                return;

            var wd = new WordAndDefinition();
            wd.Index.Text = (Count + 1).ToString();
            wd.CloseButton.Click += (object sender, RoutedEventArgs e) =>
            {
                if (WordsStackPanel.Children.Count <= 1)
                    return;

                WordsStackPanel.Children.Remove(wd);

                for (var i = 0; i < Count; ++i)
                {
                    (WordsStackPanel.Children[i] as WordAndDefinition).Index.Text = (i + 1).ToString();
                }

                CountChange?.Invoke(this, EventArgs.Empty);
            };
            WordsStackPanel.Children.Add(wd);
            CountChange?.Invoke(this, EventArgs.Empty);
        }

        void AddButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AddWordAndDefinition();
        }

        public void Resize(int wordsAmount)
        {
            if (wordsAmount < Count)
            {
                WordsStackPanel.Children.RemoveRange(wordsAmount, Count - wordsAmount);
            }
            else
            {
                for (var i = 0; Count < wordsAmount; ++i)
                {
                    AddWordAndDefinition();
                }
            }

            CountChange?.Invoke(this, EventArgs.Empty);
        }
    }
}