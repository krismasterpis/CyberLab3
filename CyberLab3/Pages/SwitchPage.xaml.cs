using CyberLab3.Resources.Libraries;
using CyberLab3.Resources.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// Logika interakcji dla klasy SwitchPage.xaml
    /// </summary>
    public partial class SwitchPage : Page
    {
        SwitchPageViewModel SPVM;
        EXFO_LTB8 LTB8;
        public SwitchPage(SwitchPageViewModel _VM)
        {
            InitializeComponent();
            SPVM = _VM;
            DataContext = SPVM;
        }

        private void ConnectButt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LTB8 = new EXFO_LTB8(IpTextBox.Text.ToString());
                LTB8.Connect();
                DeviceList.ItemsSource = LTB8.InstrumentCatalog;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
