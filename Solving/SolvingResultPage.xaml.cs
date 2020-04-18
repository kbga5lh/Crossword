using System.Windows;
using System.Windows.Controls;

namespace CrosswordApp
{
    public partial class SolvingResultPage : Page
    {
        public SolvingResultPage()
        {
            InitializeComponent();
        }

        void FinishButton_OnClick(object sender, RoutedEventArgs e)
        {
            var messageBoxResult = System.Windows.MessageBox.Show(
                "Вы уверены, что хотите вернуться в меню?",
                "Выход в меню",
                System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult != MessageBoxResult.Yes)
                return;
            
            (Parent as MainWindow).Content = new MainMenuPage();
        }
    }
}