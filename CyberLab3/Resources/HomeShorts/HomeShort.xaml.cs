using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Cpu;
using ScottPlot;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace CyberLab3.Resources.HomeShorts
{
    /// <summary>
    /// Logika interakcji dla klasy HomeShort.xaml
    /// </summary>
    public partial class HomeShort : Page
    {
        public HomeShort()
        {
            InitializeComponent();
            //Monitor();
            double[] values = { 45,100 };
            cpuGaugePlot.Plot.FigureBackground.Color = ScottPlot.Color.FromColor(System.Drawing.Color.Transparent);
            var radialGaugePlot = cpuGaugePlot.Plot.Add.RadialGaugePlot(values);
            radialGaugePlot.CircularBackground = false;
            radialGaugePlot.GaugeMode = ScottPlot.RadialGaugeMode.SingleGauge;
            radialGaugePlot.ShowLevels = false;
            radialGaugePlot.Colors[0] = ScottPlot.Color.FromColor(System.Drawing.Color.Blue);
            radialGaugePlot.Colors[1] = ScottPlot.Color.FromColor(System.Drawing.Color.Gray);
            var text = cpuGaugePlot.Plot.Add.Text("45%", 0, 0);
            text.LabelFontColor = ScottPlot.Color.FromColor(System.Drawing.Color.Blue);
            text.LabelFontSize = 26;
            text.Alignment = Alignment.MiddleCenter;
        }

        public void Monitor()
        {
            Computer computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsNetworkEnabled = true,
                IsStorageEnabled = true
            };

            computer.Open();
            computer.Accept(new UpdateVisitor());

            foreach (IHardware hardware in computer.Hardware)
            {
                Debug.WriteLine("Hardware: {0}", hardware.Name);

                foreach (IHardware subhardware in hardware.SubHardware)
                {
                    Debug.WriteLine("Subhardware: {0}", subhardware.Name);

                    foreach (ISensor sensor in subhardware.Sensors)
                    {
                        Debug.WriteLine("Sensor: {0}, value: {1}", sensor.Name, sensor.Value);
                    }
                }

                foreach (ISensor sensor in hardware.Sensors)
                {
                    Debug.WriteLine("Sensor: {0}, value: {1}", sensor.Name, sensor.Value);
                }
            }

            computer.Close();
        }
    }
}
public class UpdateVisitor : IVisitor
{
    public void VisitComputer(IComputer computer)
    {
        computer.Traverse(this);
    }
    public void VisitHardware(IHardware hardware)
    {
        hardware.Update();
        foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
    }
    public void VisitSensor(ISensor sensor) { }
    public void VisitParameter(IParameter parameter) { }
}
