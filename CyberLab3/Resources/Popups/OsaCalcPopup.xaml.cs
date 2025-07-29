using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CyberLab3.Resources.Popups
{
    /// <summary>
    /// Logika interakcji dla klasy OsaCalcPopup.xaml
    /// </summary>
    public partial class OsaCalcPopup : Window
    {
        char char1;
        char char2;
        List<Button> topButtons = new List<Button> ();
        List<Button> botButtons = new List<Button>();
        public string str;
        public OsaCalcPopup()
        {
            InitializeComponent();
            topButtons.Add(ButtA);
            topButtons.Add(ButtB);
            topButtons.Add(ButtC);
            topButtons.Add(ButtD);
            topButtons.Add(ButtE);
            topButtons.Add(ButtF);
            topButtons.Add(ButtG);
            topButtons.Add(ButtH);
            topButtons.Add(ButtI);
            topButtons.Add(ButtJ);
            botButtons.Add(ButtA2);
            botButtons.Add(ButtB2);
            botButtons.Add(ButtC2);
            botButtons.Add(ButtD2);
            botButtons.Add(ButtE2);
            botButtons.Add(ButtF2);
            botButtons.Add(ButtG2);
            botButtons.Add(ButtH2);
            botButtons.Add(ButtI2);
            botButtons.Add(ButtJ2);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button butt)
            {
                char temp = butt.Content.ToString()[0];
                if(butt.Name.Last() == '2')
                {
                    char2 = temp;
                    if(char1 == char2)
                    {
                        char1 = '\0';
                    }
                }
                else
                {
                    char1 = temp;
                    if (char1 == char2)
                    {
                        char2 = '\0';
                    }
                }
                foreach (var button in botButtons)
                {
                    if (button.Content.ToString()[0] == char2)
                    {
                        button.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(42, 132, 241));
                    }
                    else
                    {
                        button.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                    }
                }
                foreach (var button in topButtons)
                {
                    if (button.Content.ToString()[0] == char1)
                    {
                        button.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(42, 132, 241));
                    }
                    else
                    {
                        button.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                    }
                }
                if (char1 == '\0')
                {
                    textBlock.Text = $"Calculation between traces ... and {char2}";
                }
                else if(char2 == '\0')
                {
                    textBlock.Text = $"Calculation between traces {char1} and ...";
                }
                else
                {
                    textBlock.Text = $"Calculation between traces {char1} and {char2}";
                }
                if (char.IsAsciiLetterUpper(char1) && char.IsAsciiLetterUpper(char2))
                {
                    okButt.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(42, 132, 241));
                }
                else
                {
                    okButt.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                }
            }
        }

        private void okButt_Click(object sender, RoutedEventArgs e)
        {
            str = $"{char1}-{char2}";
            this.DialogResult = true;
            this.Close();
        }
    }
}
