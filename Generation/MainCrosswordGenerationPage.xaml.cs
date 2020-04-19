using System.Windows.Controls;

namespace CrosswordApp
{
    public partial class MainCrosswordGenerationPage : Page
    {
        readonly WordsPage wordsPage = new WordsPage();

        public MainCrosswordGenerationPage()
        {
            InitializeComponent();

            Content = wordsPage;
        }

        public void ToWordsPage()
        {
            Content = wordsPage;
        }

        public void ToCrosswordPage(CrosswordPage crosswordPage)
        {
            Content = crosswordPage;
        }

        public void ToMenuPage()
        {
            (Parent as MainWindow).Content = new MainMenuPage();
        }
    }
}