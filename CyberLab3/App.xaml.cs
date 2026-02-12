using CyberLab3.Resources.Services;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace CyberLab3
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public bool IsDebugEnabled = false;
        public static bool isThermalChamberActive = false;
        public static double globalTemperatureTC = -100;
        public TimerEventService TimerService { get; } = new TimerEventService();
    }

}
