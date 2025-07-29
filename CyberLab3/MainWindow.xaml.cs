using CyberLab3.Pages;
using CyberLab3.Resources.Controls;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CyberLab3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TimerViewModel Timer { get; } = new TimerViewModel();
        AttenuationPage attenuationPage = new AttenuationPage();
        HomePage HomePage = new HomePage();
        InterrogatorPage InterrogatorPage = new InterrogatorPage();
        LasersPage LasersPage = new LasersPage();
        OsaPage OsaPage = new OsaPage();
        SwitchPage SwitchPage = new SwitchPage();
        TemperaturePage TemperaturePage= new TemperaturePage();
        ThermalChamberPage ThermalChamberPage = new ThermalChamberPage();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = Timer;
            //Timer.estTime = new TimeSpan(10, 0, 0);
            sideBar.SelectedIndex = 0;
        }

        private void sideBar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = sideBar.SelectedItem as NavButton;
            if(selected != null && navFrame != null)
            {
                switch(selected.NavName)
                {
                    case "Home":
                        navFrame.Navigate(HomePage);
                        break;
                    case "OSA":
                        navFrame.Navigate(OsaPage);
                        break;
                    case "Thermal Cahmber":
                        navFrame.Navigate(ThermalChamberPage);
                        break;
                    case "Temperature Meas.":
                        navFrame.Navigate(TemperaturePage);
                        break;
                    case "Lasers":
                        navFrame.Navigate(LasersPage);
                        break;
                    case "Switch":
                        navFrame.Navigate(SwitchPage);
                        break;
                    case "Interrogator":
                        navFrame.Navigate(InterrogatorPage);
                        break;
                    case "Attenuation Meas.":
                        navFrame.Navigate(attenuationPage);
                        break;
                }
                //navFrame.Navigate(selected.NavLink);
                navInfoBar.Text = selected.NavName;
            }
        }
    }
}