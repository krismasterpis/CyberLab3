using CyberLab3.ViewModels;
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
    /// Logika interakcji dla klasy LasersPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        SettingsViewModel STVM;
        public SettingsPage(SettingsViewModel _VM)
        {
            InitializeComponent();
            STVM = _VM;
            DataContext = STVM;
        }
    }
}
