﻿using System;
using System.Windows;
using System.Windows.Controls;

namespace CrosswordApp
{
    public partial class WordsAndDefinitions : UserControl
    {
        public event EventHandler CountChange;
        
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
            if (wordsAmount < WordsStackPanel.Children.Count)
            {
                WordsStackPanel.Children.RemoveRange(wordsAmount, WordsStackPanel.Children.Count - wordsAmount);
            }
            else
            {
                for (var i = 0; WordsStackPanel.Children.Count < wordsAmount; ++i)
                {
                    AddWordAndDefinition();
                }
            }
            CountChange?.Invoke(this, EventArgs.Empty);
        }
    }
}