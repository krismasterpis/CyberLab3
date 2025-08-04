using CyberLab3.Resources.Controls;
using CyberLab3.Resources.HomeShorts;
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
    /// Logika interakcji dla klasy HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        HomePageViewModel HPVM;
        public HomePage(HomePageViewModel _VM)
        {
            InitializeComponent();
            HPVM = _VM;
            DataContext = HPVM;
        }

        private void IconTile_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                IconTile tile = e.Source as IconTile;
                DragDrop.DoDragDrop(tile, new DataObject(DataFormats.Serializable,tile), DragDropEffects.Move);
            }
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);
            Frame _frame = e.Source as Frame;
            HomeShort Hshort = new HomeShort();
            _frame.Navigate(Hshort);
        }
    }
}
