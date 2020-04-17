using System.Windows;

namespace CrosswordGenerator
{
    public partial class ConfirmationDialog : Window
    {
        public ConfirmationDialog(string title, string content)
        {
            InitializeComponent();

            TitleTextBlock.Text = title;
            ContentTextBlock.Text = content;
            
            CenterWindowOnScreen();
        }

        void CenterWindowOnScreen()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var windowWidth = Width;
            var windowHeight = Height;
            Left = screenWidth / 2 - windowWidth / 2;
            Top = screenHeight / 2 - windowHeight / 2;
        }

        void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
        
        void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}