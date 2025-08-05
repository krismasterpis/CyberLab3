using CyberLab3.Resources.Controls;
using CyberLab3.Resources.Libraries;
using CyberLab3.Resources.Popups;
using ScottPlot;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private static readonly Regex rgx = new Regex("[0-9]+");

        Stopwatch sw = new Stopwatch();
        ThermalChamberViewModel TCVM;
        ThermalChamber TC;
        List<double> temperatureSetpointsY = new List<double>() { 0, 0, 0, 0, 0 };
        List<double> temperatureSetpointsX = new List<double>() { 1, 2, 3, 4, 5 };
        List<float> temperatureX = new List<float>();
        List<float> temperatureY = new List<float>();
        List<int> elapsedMSList = new List<int>();

        private Scatter setpointsScatter;
        private Scatter temperatureScatter;

        int localTimerInterval;
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
            temperatureScatter = TCVM.MainPlot.Plot.Add.Scatter(temperatureX, temperatureY);
            temperatureScatter.MarkerStyle.Size = 10;
            TCVM.MainPlot.Refresh();
            TCVM.SetPointsPlot.Refresh();
        }

        private void ConnectButt_Click(object sender, RoutedEventArgs e)
        {
            TC = new ThermalChamber("172.16.2.217");
            TC.Connect();
            Debug.WriteLine(TC.ReadOperatingMode().ToString());
            TCVM.IsConnected= TC.IsConnected;
        }

        private void DefineSetpointButt_Click(object sender, RoutedEventArgs e)
        {
            SetpointsPopup popup = new SetpointsPopup(temperatureSetpointsX, temperatureSetpointsY);
            bool? result = popup.ShowDialog();
            TCVM.SetPointsPlot.Plot.Axes.AutoScale();
            TCVM.SetPointsPlot.Refresh();
        }

        private async void StartButt_Click(object sender, RoutedEventArgs e)
        {
            if (!TCVM.IsIntervalSetted)
            {
                TCVM.LocalTimer.Stop();
            }
            else
            {
                var interv = long.Parse(IntervalTextBox.Text);
                if (interv > 1)
                {
                    TCVM.LocalTimer = new LocalTimer(interv, LocalTimerElapsed);
                    await Task.Run(() =>
                    {
                        var status = true;
                        while (status)
                        {
                            try
                            {
                                temperatureX.Add(temperatureY.Count() * localTimerInterval);
                                temperatureY.Add(TC.ReadTemperature());
                                status = false;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                        }
                    });
                    TCVM.MainPlot.Plot.Axes.AutoScale();
                    TCVM.MainPlot.Refresh();
                    TCVM.LocalTimer.Reset();
                    TCVM.LocalTimer.Start();
                }
                else
                {
                    IntervalTextBox.Text = null;
                }
            }
        }
        private async void LocalTimerElapsed(object? sender, EventArgs e)
        {
            if (TCVM.LocalTimer != null)
            {
                await Task.Run(() =>
                {
                    var status = true;
                    sw.Restart();
                    while (status)
                    {
                        try
                        {
                            var temp = TC.ReadTemperature();
                            temperatureX.Add(temperatureY.Count() * localTimerInterval);
                            temperatureY.Add(temp);
                            status = false;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                            TCVM.MeasurementFails += 1;
                        }
                    }
                    sw.Stop();
                    elapsedMSList.Add((int)sw.ElapsedMilliseconds);
                    TCVM.ElapsedMsAvg = elapsedMSList.Average();
                    TCVM.ElapsedMs = sw.ElapsedMilliseconds;
                });
                TCVM.MainPlot.Plot.Axes.AutoScale();
                TCVM.MainPlot.Refresh();
            }
        }

        private void IntervalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox temp = sender as TextBox;
            if(temp != null)
            {
                var result = rgx.IsMatch(temp.Text);
                int.TryParse(temp.Text, out var intResult);
                if(intResult>=1)
                {
                    TCVM.IsIntervalSetted = result;
                    localTimerInterval = intResult;
                }
                else
                {
                    TCVM.IsIntervalSetted = false;
                }
            }
        }
    }
}
