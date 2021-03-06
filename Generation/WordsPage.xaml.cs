﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrosswordApp
{
    public partial class WordsPage : UserControl
    {
        public WordsPage()
        {
            InitializeComponent();

            WordsAndDefinitionsElement.CountChange += (sender, args) =>
            {
                WordsAmountTextBox.Text = WordsAndDefinitionsElement.Count.ToString();
                WordsAmountTextBox.SelectionStart = WordsAmountTextBox.Text.Length;
            };
        }

        void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var messageBoxResult = MessageBox.Show(
                "Вы уверены, что хотите вернуться в меню? Все несохраненные изменения пропадут.", "Выход в меню",
                MessageBoxButton.YesNo);
            if (messageBoxResult != MessageBoxResult.Yes)
                return;

            (Parent as MainCrosswordGenerationPage).ToMenuPage();
        }

        void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            var words = ReadFromTextBoxes();
            if (words == null)
                return;

            (Parent as MainCrosswordGenerationPage).ToCrosswordPage(new CrosswordPage(words, NameTextBox.Text));
        }

        List<(string, string)> ReadFromTextBoxes()
        {
            var list = new List<(string, string)>();
            foreach (var v in WordsAndDefinitionsElement.WordsStackPanel.Children)
            {
                if (!(v is WordAndDefinition wd)) continue;
                if (string.IsNullOrEmpty(wd.WordTextBox.Text))
                {
                    MessageBox.Show("Не все поля заполнены!");
                    wd.WordTextBox.Focus();
                    return null;
                }

                if (string.IsNullOrEmpty(wd.DefinitionTextBox.Text))
                {
                    MessageBox.Show("Не все поля заполнены!");
                    wd.DefinitionTextBox.Focus();
                    return null;
                }

                list.Add((wd.WordTextBox.Text, wd.DefinitionTextBox.Text));
            }

            return list;
        }

        static List<(string, string)> ReadFromFile(string path)
        {
            var jsonWords = JsonConvert.DeserializeObject(File.ReadAllText(path)) as JArray;
            var words = jsonWords
                .Select(p => ((string) p["Item1"], (string) p["Item2"]))
                .ToList();
            return words;
        }

        void FillButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var fileDialog = new OpenFileDialog {Filter = "JSON file|*.json"};
                if (fileDialog.ShowDialog() != true)
                    return;

                var words = ReadFromFile(fileDialog.FileName);
                WordsAndDefinitionsElement.WordsStackPanel.Children.Clear();
                foreach (var (word, definition) in words)
                {
                    WordsAndDefinitionsElement.AddWordAndDefinition();
                    var wd = WordsAndDefinitionsElement.WordsStackPanel.Children[
                        WordsAndDefinitionsElement.Count - 1] as WordAndDefinition;
                    wd.WordTextBox.Text = word;
                    wd.DefinitionTextBox.Text = definition;
                }

                WordsAmountTextBox.Text = WordsAndDefinitionsElement.Count.ToString();
            }
            catch (Exception)
            {
                MessageBox.Show("Произошла ошибка при считывании данных из файла");
            }
        }

        void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var fileDialog = new SaveFileDialog {Filter = "JSON file|*.json"};
                if (fileDialog.ShowDialog() != true)
                    return;

                var words = ReadFromTextBoxes();
                var jsonWords = JsonConvert.SerializeObject(words);

                File.WriteAllText(fileDialog.FileName, jsonWords);
            }
            catch (Exception)
            {
                MessageBox.Show("Произошла ошибка при записи данных в файл");
            }
        }

        void WordsAmountTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return) return;
            ResizeWordsAndDefinitions();
        }

        void ResizeWordsAndDefinitions()
        {
            if (!int.TryParse(WordsAmountTextBox.Text, out var wordsAmount))
            {
                MessageBox.Show("Вы ввели не число");
                WordsAmountTextBox.Text = WordsAndDefinitionsElement.Count.ToString();
                WordsAmountTextBox.SelectionStart = WordsAmountTextBox.Text.Length;
                return;
            }

            if (wordsAmount < 1 || wordsAmount > WordsAndDefinitions.maxCount)
            {
                MessageBox.Show("Количество слов должно быть > 0 и <= 500");
                WordsAmountTextBox.Text = WordsAndDefinitionsElement.Count.ToString();
                WordsAmountTextBox.SelectionStart = WordsAmountTextBox.Text.Length;
            }
            else
            {
                WordsAndDefinitionsElement.Resize(wordsAmount);
            }
        }

        void WordsAmountTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            ResizeWordsAndDefinitions();
        }
    }
}