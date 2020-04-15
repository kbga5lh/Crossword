using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace CrosswordApp
{
    public partial class WordsPage : UserControl
    {
        public WordsPage()
        {
            InitializeComponent();

            WordsAndDefinitionsElement.CountChange += (sender, args) =>
            {
                WordsAmountTextBox.Text = WordsAndDefinitionsElement.WordsStackPanel.Children.Count.ToString();
            };
        }

        void BackButton_Click(object sender, RoutedEventArgs e)
        {
            (Parent as MainCrosswordGenerationPage).ToMenuPage();
        }

        void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            var words = ReadFromTextBoxes();

            (Parent as MainCrosswordGenerationPage).ToCrosswordPage(new CrosswordPage(words, NameTextBox.Text));
        }

        List<(string, string)> ReadFromTextBoxes()
        {
            return (from WordAndDefinition v
                in WordsAndDefinitionsElement.WordsStackPanel.Children
                select (v.WordTextBox.Text, v.DefinitionTextBox.Text)).ToList();
        }

        static List<(string, string)> ReadFromFile(string path)
        {
            var jsonWords = JsonConvert.DeserializeObject(File.ReadAllText(path)) as Newtonsoft.Json.Linq.JArray;
            var words = jsonWords
                .Select(p => ((string)p["Item1"], (string)p["Item2"]))
                .ToList();
            return words;
        }

        void FillButton_OnClick(object sender, RoutedEventArgs e)
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
                    WordsAndDefinitionsElement.WordsStackPanel.Children.Count - 1] as WordAndDefinition;
                wd.WordTextBox.Text = word;
                wd.DefinitionTextBox.Text = definition;
            }

            WordsAmountTextBox.Text = WordsAndDefinitionsElement.WordsStackPanel.Children.Count.ToString();
        }

        void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new SaveFileDialog {Filter = "JSON file|*.json"};
            if (fileDialog.ShowDialog() != true)
                return;
            
            var words = ReadFromTextBoxes();
            var jsonWords = JsonConvert.SerializeObject(words);
            
            File.WriteAllText(fileDialog.FileName, jsonWords);
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
                return;
            }

            if (wordsAmount < 1 || wordsAmount > 500)
            {
                return;
            }
            WordsAndDefinitionsElement.Resize(wordsAmount);
        }

        void WordsAmountTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            ResizeWordsAndDefinitions();
        }
    }
}