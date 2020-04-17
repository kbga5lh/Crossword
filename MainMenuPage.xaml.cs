using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace CrosswordApp
{
    public partial class MainMenuPage : Page
    {
        public MainMenuPage()
        {
            InitializeComponent();
        }

        void GenerateCrosswordButton_OnClick(object sender, RoutedEventArgs e)
        {
            (Parent as MainWindow).Content = new MainCrosswordGenerationPage();
        }

        void SolveCrosswordButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var fileDialog = new OpenFileDialog
                {
                    Filter = "JSON file|*.json",
                };
                if (fileDialog.ShowDialog() != true)
                    return;

                var jsonCrossword = File.ReadAllText(fileDialog.FileName);
                var crossword = JsonConvert.DeserializeObject<Crossword>(jsonCrossword);
                (Parent as MainWindow).Content = new CrosswordSolvingPage(crossword);
            }
            catch (Exception)
            {
                MessageBox.Show("Произошла ошибка при считывании данных из файла");
            }
        }
    }
}