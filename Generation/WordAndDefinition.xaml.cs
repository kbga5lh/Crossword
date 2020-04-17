using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CrosswordApp
{
    /// <summary>
    /// Логика взаимодействия для WordAndDefinition.xaml
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
                    ++i;
            }

            if (formattedText != tb.Text)
            {
                tb.Text = formattedText;
                tb.SelectionStart = selectionStart;
            }
        }
    }
}
