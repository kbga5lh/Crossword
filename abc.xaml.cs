using System.Windows;

namespace CrosswordGenerator
{
    public partial class abc : Window
    {
        public abc()
        {
            InitializeComponent();

            Closing += (sender, args) => DialogResult = true;
        }
    }
}