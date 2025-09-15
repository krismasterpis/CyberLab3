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
    /// Logika interakcji dla klasy DatabasePage.xaml
    /// </summary>
    public partial class DatabasePage : Page
    {
        DatabasePageViewModel DBVM;
        public DatabasePage(DatabasePageViewModel _viewModel)
        {
            InitializeComponent();
            DBVM = _viewModel;
            DataContext = DBVM;
        }
    }
}
