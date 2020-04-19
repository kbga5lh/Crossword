using System.Windows.Controls;

namespace CrosswordApp
{
    /// <summary>
    ///     Логика взаимодействия для WordAndDefinition.xaml
    /// </summary>
    public partial class WordAndDefinition : UserControl
    {
        public WordAndDefinition()
        {
            InitializeComponent();
        }

        void WordTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox tb)) return;
            var selectionStart = tb.SelectionStart;

            var formattedText = tb.Text.ToLower();
            for (var i = 0; i < formattedText.Length;)
            {
                var ch = formattedText[i];
                if (ch < 'а' || ch > 'я')
                {
                    formattedText = formattedText.Remove(i, 1);
                    if (i < selectionStart)
                        --selectionStart;
                }
                else
                {
                    ++i;
                }
            }

            if (formattedText != tb.Text)
            {
                tb.Text = formattedText;
                tb.SelectionStart = selectionStart;
            }
        }
    }
}