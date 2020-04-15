using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace CrosswordApp
{
    public partial class WordsPage : Page
    {
        public WordsPage()
        {
            InitializeComponent();
        }

        void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            var words = ReadFromTextBoxes();

            (Parent as MainWindow).Content = new CrosswordPage(words, NameTextBox.Text);
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
            foreach (var word in words)
            {
                WordsAndDefinitionsElement.AddWordAndDefinition();
                var wd = WordsAndDefinitionsElement.WordsStackPanel.Children[
                    WordsAndDefinitionsElement.WordsStackPanel.Children.Count - 1] as WordAndDefinition;
                wd.WordTextBox.Text = word.Item1;
                wd.DefinitionTextBox.Text = word.Item2;
            }
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
    }
}