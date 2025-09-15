using CyberLab3.Pages;
using CyberLab3.Resources.Controls;
using CyberLab3.ViewModels;
using SqliteWpfApp;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Runtime.InteropServices;
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
    [FlagsAttribute]
    public enum EXECUTION_STATE : uint
    {
        ES_AWAYMODE_REQUIRED = 0x00000040,
        ES_CONTINUOUS = 0x80000000,
        ES_DISPLAY_REQUIRED = 0x00000002,
        ES_SYSTEM_REQUIRED = 0x00000001
        // Legacy flag, should not be used.
        // ES_USER_PRESENT = 0x00000004
    }
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
        //ViewModels
        AttenuationPageViewModel AttenuationPageVM = new AttenuationPageViewModel();
        HomePageViewModel HomePageVM = new HomePageViewModel();
        InterrogatorPageViewModel InterrogatorPageVM = new InterrogatorPageViewModel();
        LasersPageViewModel LasersPageVM = new LasersPageViewModel();
        OsaPageViewModel OsaPageVM = new OsaPageViewModel();
        SwitchPageViewModel SwitchPageVM = new SwitchPageViewModel();
        TemperaturePageViewModel TemperaturePageVM = new TemperaturePageViewModel();
        ThermalChamberViewModel ThermalChamberPageVM = new ThermalChamberViewModel();
        CyberLabPageViewModel CyberLabPageViewModel = new CyberLabPageViewModel();
        DatabasePageViewModel DatabasePageViewModel = new DatabasePageViewModel();
        //Strony
        AttenuationPage attenuationPage_;
        HomePage HomePage_;
        InterrogatorPage InterrogatorPage_;
        LasersPage LasersPage_;
        OsaPage OsaPage_;
        SwitchPage SwitchPage_;
        TemperaturePage TemperaturePage_;
        ThermalChamberPage ThermalChamberPage_;
        CyberLabPage CyberLabPage_;
        DatabasePage DatabasePage_;
        public DatabaseContext db = new DatabaseContext();
        public TimerViewModel TimerVM { get; } = new TimerViewModel();
        public MainWindow()
        {
            InitializeComponent();
            PreventSleep();
            db.Database.EnsureCreated(); // Tworzy bazę danych, jeśli nie istnieje
            DataContext = TimerVM;
            attenuationPage_ = new AttenuationPage(AttenuationPageVM);
            HomePage_ = new HomePage(HomePageVM);
            InterrogatorPage_ = new InterrogatorPage(InterrogatorPageVM);
            LasersPage_ = new LasersPage(LasersPageVM);
            OsaPage_ = new OsaPage(OsaPageVM, db);
            SwitchPage_ = new SwitchPage(SwitchPageVM);
            TemperaturePage_ = new TemperaturePage(TemperaturePageVM);
            ThermalChamberPage_ = new ThermalChamberPage(ThermalChamberPageVM);
            CyberLabPage_ = new CyberLabPage(CyberLabPageViewModel);
            DatabasePage_ = new DatabasePage(DatabasePageViewModel);
            sideBar.SelectedIndex = 0;
        }
        void PreventSleep()
        {
            // Prevent Idle-to-Sleep (monitor not affected) (see note above)
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_AWAYMODE_REQUIRED);
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
                    case "CyberLab":
                        navFrame.Navigate(CyberLabPage_);
                        break;
                    case "Database":
                        navFrame.Navigate(DatabasePage_);
                        break;
                }
                //navFrame.Navigate(selected.NavLink);
                navInfoBar.Text = selected.NavName;
            }
        }
    }
}