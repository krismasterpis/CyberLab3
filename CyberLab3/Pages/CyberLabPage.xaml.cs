using CyberLab3.Resources.Nodes;
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
    /// Logika interakcji dla klasy CyberLabPage.xaml
    /// </summary>
    public partial class CyberLabPage : Page
    {
        CyberLabPageViewModel CLVM;
        public CyberLabPage(CyberLabPageViewModel _viewModel)
        {
            InitializeComponent();
            CLVM = _viewModel;
            DataContext = new EditorViewModel();
        }
    }
}
