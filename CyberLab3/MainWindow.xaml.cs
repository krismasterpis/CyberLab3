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
        //ViewModels
        AttenuationPageViewModel AttenuationPageVM = new AttenuationPageViewModel();
        HomePageViewModel HomePageVM = new HomePageViewModel();
        InterrogatorPageViewModel InterrogatorPageVM = new InterrogatorPageViewModel();
        LasersPageViewModel LasersPageVM = new LasersPageViewModel();
        OsaPageViewModel OsaPageVM = new OsaPageViewModel();
        SwitchPageViewModel SwitchPageVM = new SwitchPageViewModel();
        TemperaturePageViewModel TemperaturePageVM = new TemperaturePageViewModel();
        ThermalChamberViewModel ThermalChamberPageVM = new ThermalChamberViewModel();
        //Strony
        AttenuationPage attenuationPage_;
        HomePage HomePage_;
        InterrogatorPage InterrogatorPage_;
        LasersPage LasersPage_;
        OsaPage OsaPage_;
        SwitchPage SwitchPage_;
        TemperaturePage TemperaturePage_;
        ThermalChamberPage ThermalChamberPage_;
        public TimerViewModel TimerVM { get; } = new TimerViewModel();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = TimerVM;
            attenuationPage_ = new AttenuationPage(AttenuationPageVM);
            HomePage_ = new HomePage(HomePageVM);
            InterrogatorPage_ = new InterrogatorPage(InterrogatorPageVM);
            LasersPage_ = new LasersPage(LasersPageVM);
            OsaPage_ = new OsaPage(OsaPageVM);
            SwitchPage_ = new SwitchPage(SwitchPageVM);
            TemperaturePage_ = new TemperaturePage(TemperaturePageVM);
            ThermalChamberPage_ = new ThermalChamberPage(ThermalChamberPageVM);
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
                        navFrame.Navigate(HomePage_);
                        break;
                    case "OSA":
                        navFrame.Navigate(OsaPage_);
                        break;
                    case "Thermal Cahmber":
                        navFrame.Navigate(ThermalChamberPage_);
                        break;
                    case "Temperature Meas.":
                        navFrame.Navigate(TemperaturePage_);
                        break;
                    case "Lasers":
                        navFrame.Navigate(LasersPage_);
                        break;
                    case "Switch":
                        navFrame.Navigate(SwitchPage_);
                        break;
                    case "Interrogator":
                        navFrame.Navigate(InterrogatorPage_);
                        break;
                    case "Attenuation Meas.":
                        navFrame.Navigate(attenuationPage_);
                        break;
                }
                //navFrame.Navigate(selected.NavLink);
                navInfoBar.Text = selected.NavName;
            }
        }
    }
}