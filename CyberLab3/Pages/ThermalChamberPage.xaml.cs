using CyberLab3.Resources.Popups;
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

namespace CyberLab3.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy ThermalChamberPage.xaml
    /// </summary>
    public partial class ThermalChamberPage : Page
    {
        ThermalChamberViewModel TCVM;
        ThermalChamber TC;
        List<double> temperatureSetpointsY = new List<double>() {0,0,0,0,0}; 
        List<double> temperatureSetpointsX = new List<double>() {1,2,3,4,5};

        private Scatter setpointsScatter;
        public ThermalChamberPage(ThermalChamberViewModel _VM)
        {
            InitializeComponent();
            TCVM = _VM;
            DataContext = TCVM;
            TCVM.MainPlot.Plot.FigureBackground.Color = ScottPlot.Color.FromColor(System.Drawing.Color.Transparent);
            TCVM.SetPointsPlot.Plot.FigureBackground.Color = ScottPlot.Color.FromColor(System.Drawing.Color.Transparent);
            PixelPadding padding = new PixelPadding(75, 35, 75, 35);
            TCVM.MainPlot.Plot.Layout.Fixed(padding);
            TCVM.SetPointsPlot.Plot.Layout.Fixed(padding);
            TCVM.MainPlot.Plot.Axes.Left.Label.Text = "Vertical Axis";
            TCVM.SetPointsPlot.Plot.Axes.Left.Label.Text = "Vertical Axis";
            TCVM.MainPlot.Plot.Axes.Left.Label.FontSize = 20;
            TCVM.SetPointsPlot.Plot.Axes.Left.Label.FontSize = 20;
            TCVM.MainPlot.Plot.Axes.Left.TickLabelStyle.FontSize = 16;
            TCVM.SetPointsPlot.Plot.Axes.Left.TickLabelStyle.FontSize = 16;
            TCVM.MainPlot.Plot.Axes.Bottom.Label.Text = "Horizontal Axis";
            TCVM.SetPointsPlot.Plot.Axes.Bottom.Label.Text = "Horizontal Axis";
            TCVM.MainPlot.Plot.Axes.Bottom.Label.FontSize = 20;
            TCVM.SetPointsPlot.Plot.Axes.Bottom.Label.FontSize = 20;
            TCVM.MainPlot.Plot.Axes.Bottom.TickLabelStyle.FontSize = 16;
            TCVM.SetPointsPlot.Plot.Axes.Bottom.TickLabelStyle.FontSize = 16;
            TCVM.SetPointsPlot.UserInputProcessor.UserActionResponses.Clear();
            setpointsScatter = TCVM.SetPointsPlot.Plot.Add.Scatter(temperatureSetpointsX, temperatureSetpointsY);
            setpointsScatter.LineWidth = 3;
            setpointsScatter.MarkerStyle.Size = 10;
            setpointsScatter.MarkerStyle.FillColor = ScottPlot.Color.FromColor(System.Drawing.Color.Blue);
            TCVM.MainPlot.Refresh();
            TCVM.SetPointsPlot.Refresh();
        }

        private void ConnectButt_Click(object sender, RoutedEventArgs e)
        {
            TC = new ThermalChamber("172.16.2.217");
            TC.Connect();
            Debug.WriteLine(TC.ReadOperatingMode().ToString());
        }

        private void DefineSetpointButt_Click(object sender, RoutedEventArgs e)
        {
            SetpointsPopup popup = new SetpointsPopup(temperatureSetpointsX, temperatureSetpointsY);
            bool? result = popup.ShowDialog();
            TCVM.SetPointsPlot.Plot.Axes.AutoScale();
            TCVM.SetPointsPlot.Refresh();
        }
    }
}
