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
        public static bool isThermalChamberActive = false;
        public static double globalTemperatureTC = -100;
    }

}
