using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace CrosswordApp
{
    public partial class WordsPage : Page
    {
        public WordsPage()
        {
            InitializeComponent();
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            List<(string, string)> words = ReadFromFile("words.json");

            (Parent as MainWindow).Content = new CrosswordPage(words);
        }

        private List<(string, string)> ReadFromTextBoxes()
        {
            var words = new List<(string, string)>();

            foreach (WordAndDefinition v in WordsAndDefinitionsElement.WordsStackPanel.Children)
            {
                words.Add((v.WordTextBox.Text, v.DefinitionTextBox.Text));
            }

            return words;
        }

        private static List<(string, string)> ReadFromFile(string path)
        {
            var jsonWords = JsonConvert.DeserializeObject(File.ReadAllText(path)) as Newtonsoft.Json.Linq.JArray;
            var words = jsonWords
                .Select(p => ((string)p["Item1"], (string)p["Item2"]))
                .ToList();
            return words;
        }
    }
}