using System.Windows;
using System.Windows.Controls;

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
    }
}