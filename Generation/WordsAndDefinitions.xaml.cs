using System;
using System.Windows;
using System.Windows.Controls;

namespace CrosswordApp
{
    public partial class WordsAndDefinitions : UserControl
    {
        public WordsAndDefinitions()
        {
            InitializeComponent();

            AddWordAndDefinition();
        }

        public void AddWordAndDefinition()
        {
            var wd = new WordAndDefinition();
            wd.Index.Text = (WordsStackPanel.Children.Count + 1).ToString();
            wd.CloseButton.Click += (object sender, RoutedEventArgs e) =>
            {
                if (WordsStackPanel.Children.Count <= 1)
                    return;

                WordsStackPanel.Children.Remove(wd);

                for (var i = 0; i < WordsStackPanel.Children.Count; ++i)
                {
                    (WordsStackPanel.Children[i] as WordAndDefinition).Index.Text = (i + 1).ToString();
                }
            };
            WordsStackPanel.Children.Add(wd);
        }

        void AddButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AddWordAndDefinition();
        }
    }
}