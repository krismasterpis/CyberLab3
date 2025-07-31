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

namespace CyberLab3.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy InterrogatorPage.xaml
    /// </summary>
    public partial class InterrogatorPage : Page
    {
        InterrogatorPageViewModel IPVM;
        public InterrogatorPage(InterrogatorPageViewModel _VM)
        {
            InitializeComponent();
            IPVM = _VM;
            DataContext = IPVM;
        }
    }
}
