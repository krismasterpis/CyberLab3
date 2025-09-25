using CyberLab3.Resources.Controls;
using CyberLab3.Resources.Libraries;
using CyberLab3.Resources.Popups;
using ScottPlot;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
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
        List<SetPoint> temperatureSetpoints = new List<SetPoint>();
        List<float> temperatureX = new List<float>();
        List<float> temperatureY = new List<float>();
        List<float> errorX = new List<float>();
        List<float> errorY = new List<float>();
        List<int> elapsedMSList = new List<int>();

        private Scatter setpointsScatter;
        private Scatter temperatureScatter;
        private HorizontalLine setPointLine;
        private HorizontalLine tauLine;
        private Scatter errorScatter;

        int currentSetPointIndex = 0;
        int localSetPoint = -100;
        int intResult;
        string tempType = "Idle";
        bool measurementFailed = false;
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
            errorScatter = TCVM.MainPlot.Plot.Add.Scatter(errorX,errorY);
            errorScatter.Color = ScottPlot.Color.FromColor(System.Drawing.Color.Red);
            errorScatter.LineWidth = 0;
            errorScatter.MarkerStyle.Size = 15;
            errorScatter.MarkerStyle.Shape = MarkerShape.Cross;
            TCVM.MainPlot.Refresh();
            TCVM.SetPointsPlot.Refresh();
            TCVM.LocalTimer = new LocalTimer(TCVM.Interval, LocalTimerElapsed);
            ipTextBox.Text = "172.16.2.217";
        }
        private async void ConnectButt_Click(object sender, RoutedEventArgs e)
        {
            if(TC == null || !TC.checkIfConnected())
            {
                if (IsStrictIPv4(ipTextBox.Text))
                {
                    TC = new ThermalChamber(ipTextBox.Text);
                    await Task.Run(() =>
                    {
                        var status = true;
                        while(status)
                        {
                            try
                            {
                                TC.Connect();
                                TC.SetOperatingMode(Mb1OperatingMode.Manual);
                                TCVM.CurrSetPoint = TC.ReadSetPoint();
                                if(setPointLine == null)
                                {
                                    setPointLine = TCVM.MainPlot.Plot.Add.HorizontalLine(TCVM.CurrSetPoint);
                                    setPointLine.LineStyle.Color = ScottPlot.Color.FromHex("#ffa500");
                                    setPointLine.LineStyle.Pattern = LinePattern.Dashed;
                                }
                                else
                                {
                                    setPointLine.Position = TCVM.CurrSetPoint;
                                }
                                TCVM.IsConnected = TC.IsConnected;
                                TCVM.ThermalChamberStatus = Enum.GetName(TC.ReadOperatingMode());
                                status = false;
                            }
                            catch(Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                        }
                        TCVM.LocalTimer.Start();
                    });
                }
            }
        }
        private bool IsStrictIPv4(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            string[] parts = ip.Split('.');
            if (parts.Length != 4)
                return false;

            foreach (var part in parts)
            {
                if (!int.TryParse(part, out int byteVal))
                    return false;

                if (byteVal < 0 || byteVal > 255)
                    return false;
            }

            return true;
        }
        private void ReadTemperature()
        {
            var status = true;
            while (status)
            {
                if (!TC.checkIfConnected())
                {
                    TC.Connect();
                }
                var temp = TC.ReadTemperature();
                if (temperatureX.Count() == 0) temperatureX.Add(0);
                else temperatureX.Add(temperatureX.Last() + TCVM.Interval);
                temperatureY.Add(temp);
                TCVM.CurrTemperature = temp;
                App.isThermalChamberActive = true;
                App.globalTemperatureTC = temp;
                status = false;
            }
        }
        private void DefineSetpointButt_Click(object sender, RoutedEventArgs e)
        {
            SetpointsPopup popup = new SetpointsPopup(temperatureSetpointsX, temperatureSetpointsY, TCVM, temperatureSetpoints);
            bool? result = popup.ShowDialog();
            TCVM.SetPointsPlot.Plot.Axes.AutoScale();
            TCVM.SetPointsPlot.Refresh();
        }
        private async void LocalTimerElapsed(object? sender, EventArgs e)
        {
            if (TCVM.LocalTimer != null)
            {
                await Task.Run(() =>
                {
                    sw.Restart();
                    try
                    {
                        ReadTemperature();
                        if(measurementFailed)
                        {
                            errorX.Add(temperatureX.Last());
                            errorY.Add(temperatureY.Last());
                            measurementFailed = false;
                        }
                        if (!TC.checkIfConnected())
                        {
                            TC.Connect();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        TCVM.MeasurementFails += 1;
                        measurementFailed = true;
                    }
                    sw.Stop();
                    if(temperatureSetpoints.Count > 0)
                    {
                        if(currentSetPointIndex > 0)
                        {
                            if (temperatureSetpoints[currentSetPointIndex].timeConstS_cool > 0 || temperatureSetpoints[currentSetPointIndex].timeConstS_heat > 0)
                            {
                                if (tempType == "Heating")
                                {
                                    if (TCVM.CurrTemperature >= temperatureSetpoints[currentSetPointIndex - 1].temperature + (temperatureSetpoints[currentSetPointIndex].temperature - temperatureSetpoints[currentSetPointIndex - 1].temperature) * 0.63)
                                    {
                                        TCVM.LocalTimer2.Start();
                                    }
                                }
                                if (tempType == "Cooling")
                                {
                                    if (TCVM.CurrTemperature <= temperatureSetpoints[currentSetPointIndex - 1].temperature + (temperatureSetpoints[currentSetPointIndex].temperature - temperatureSetpoints[currentSetPointIndex - 1].temperature) * 0.63)
                                    {
                                        TCVM.LocalTimer2.Start();
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (temperatureSetpoints[currentSetPointIndex].timeConstS_cool > 0 || temperatureSetpoints[currentSetPointIndex].timeConstS_heat > 0)
                            {
                                if (tempType == "Heating")
                                {
                                    if (TCVM.CurrTemperature >= temperatureSetpoints[currentSetPointIndex].temperature - 1 && TCVM.CurrTemperature <= temperatureSetpoints[currentSetPointIndex].temperature + 1)
                                    {
                                        TCVM.LocalTimer2.Start();
                                    }
                                }
                                if (tempType == "Cooling")
                                {
                                    if (TCVM.CurrTemperature >= temperatureSetpoints[currentSetPointIndex].temperature - 1 && TCVM.CurrTemperature <= temperatureSetpoints[currentSetPointIndex].temperature + 1)
                                    {
                                        TCVM.LocalTimer2.Start();
                                    }
                                }
                            }
                        }
                    }
                    elapsedMSList.Add((int)sw.ElapsedMilliseconds);
                    TCVM.ElapsedMsAvg = elapsedMSList.Average();
                    TCVM.ElapsedMs = sw.ElapsedMilliseconds;
                });
                TCVM.MainPlot.Plot.Axes.AutoScale();
                TCVM.MainPlot.Refresh();
            }
        }
        private async void LocalTimerElapsed2(object? sender, EventArgs e)
        {
            if (TCVM.LocalTimer != null)
            {
                if (currentSetPointIndex < temperatureSetpoints.Count() - 1)
                {
                    currentSetPointIndex += 1;
                    TCVM.CurrSetPoint = (float)temperatureSetpoints[currentSetPointIndex].temperature;
                    await Task.Run(() =>
                    {
                        if (!TC.checkIfConnected())
                        {
                            TC.Connect();
                        }
                        var status = true;
                        while (status)
                        {
                            try
                            {
                                TC.SetTemperature(TCVM.CurrSetPoint);
                                if (setPointLine != null)
                                {
                                    setPointLine.Position = TCVM.CurrSetPoint;
                                }
                                status = false;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                                TCVM.MeasurementFails += 1;
                            }
                        }
                    });
                    if (temperatureSetpoints[currentSetPointIndex].timeS > 0)
                    {
                        TCVM.LocalTimer2.Stop();
                        TCVM.LocalTimer2 = new LocalTimer(temperatureSetpoints[currentSetPointIndex].timeS, LocalTimerElapsed2);
                        TCVM.LocalTimer2.Reset();
                        TCVM.LocalTimer2.Start();
                    }
                    else
                    {
                        if (temperatureSetpoints[currentSetPointIndex].timeConstS_cool > 0 || temperatureSetpoints[currentSetPointIndex].timeConstS_heat > 0)
                        {
                            var tau = temperatureSetpoints[currentSetPointIndex - 1].temperature + (temperatureSetpoints[currentSetPointIndex].temperature - temperatureSetpoints[currentSetPointIndex - 1].temperature) * 0.63;
                            if (temperatureSetpoints[currentSetPointIndex - 1].temperature <= temperatureSetpoints[currentSetPointIndex].temperature)
                            {
                                TCVM.LocalTimer2.Stop();
                                TCVM.LocalTimer2 = new LocalTimer(temperatureSetpoints[currentSetPointIndex].timeConstS_heat, LocalTimerElapsed2);
                                tempType = "Heating";
                                if (tauLine == null)
                                {
                                    tauLine = TCVM.MainPlot.Plot.Add.HorizontalLine(tau);
                                    tauLine.LineStyle.Color = ScottPlot.Color.FromHex("#008000");
                                    tauLine.LineStyle.Pattern = LinePattern.Dashed;
                                }
                                else
                                {
                                    tauLine.Position = tau;
                                }
                                TCVM.LocalTimer2.Reset();
                            }
                            else
                            {
                                TCVM.LocalTimer2.Stop();
                                TCVM.LocalTimer2 = new LocalTimer(temperatureSetpoints[currentSetPointIndex].timeConstS_cool, LocalTimerElapsed2);
                                tempType = "Cooling";
                                if (tauLine == null)
                                {
                                    tauLine = TCVM.MainPlot.Plot.Add.HorizontalLine(tau);
                                    tauLine.LineStyle.Color = ScottPlot.Color.FromHex("#008000");
                                    tauLine.LineStyle.Pattern = LinePattern.Dashed;
                                }
                                else
                                {
                                    tauLine.Position = tau;
                                }
                                TCVM.LocalTimer2.Reset();
                            }
                        }
                    }
                }
                else
                {
                    TCVM.LocalTimer2.Stop();
                }
            }
        }
        private void IntervalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox temp = sender as TextBox;
            if(temp != null)
            {
                var result = rgx.IsMatch(temp.Text);
                int.TryParse(temp.Text, out intResult);
                if(intResult>=1)
                {
                    TCVM.IsIntervalSetted = result;
                }
                else
                {
                    TCVM.IsIntervalSetted = false;
                }
            }
        }

        private async void SetButt_Click(object sender, RoutedEventArgs e)
        {
            if(localSetPoint >= -40 && localSetPoint <= 180)
            {
                await Task.Run(() =>
                {
                    if (!TC.checkIfConnected())
                    {
                        TC.Connect();
                    }
                    TC.SetTemperature(localSetPoint);
                    TCVM.CurrSetPoint = localSetPoint;
                    if(setPointLine != null)
                    {
                        setPointLine.Position = TCVM.CurrSetPoint;
                    }
                });
            }
        }

        private void SetPointTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox temp = sender as TextBox;
            if (temp != null)
            {
                var result = rgx.IsMatch(temp.Text);
                int.TryParse(temp.Text, out var intResult);
                localSetPoint = intResult;
            }
        }

        private async void StartButt_Click(object sender, RoutedEventArgs e)
        {
            if (TCVM.Interval >= 1)
            {
                if(temperatureSetpoints.Count() >= 2)
                {
                    await Task.Run(() =>
                    {
                        if (!TC.checkIfConnected())
                        {
                            TC.Connect();
                        }
                        var status = true;
                        while (status)
                        {
                            try
                            {
                                TC.SetTemperature((float)temperatureSetpoints[currentSetPointIndex].temperature);
                                TCVM.CurrSetPoint = (float)temperatureSetpoints[currentSetPointIndex].temperature;
                                if (setPointLine != null)
                                {
                                    setPointLine.Position = TCVM.CurrSetPoint;
                                }
                                status = false;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                        }
                    });
                }
                if (temperatureSetpoints[currentSetPointIndex].timeS > 0)
                {
                    if (TCVM.LocalTimer2 != null) TCVM.LocalTimer2.Stop();
                    TCVM.LocalTimer2 = new LocalTimer(temperatureSetpoints[currentSetPointIndex].timeS, LocalTimerElapsed2);
                    TCVM.LocalTimer2.Reset();
                    TCVM.LocalTimer2.Start();
                }
                else if (temperatureSetpoints[currentSetPointIndex].timeConstS_cool > 0 || temperatureSetpoints[currentSetPointIndex].timeConstS_heat > 0)
                {
                    if (TCVM.CurrTemperature <= temperatureSetpoints[currentSetPointIndex].temperature)
                    {
                        if (TCVM.LocalTimer2 != null) TCVM.LocalTimer2.Stop();
                        TCVM.LocalTimer2 = new LocalTimer(temperatureSetpoints[currentSetPointIndex].timeConstS_heat, LocalTimerElapsed2);
                        tempType = "Heating";
                        TCVM.LocalTimer2.Reset();
                    }
                    else
                    {
                        if (TCVM.LocalTimer2 != null) TCVM.LocalTimer2.Stop();
                        TCVM.LocalTimer2 = new LocalTimer(temperatureSetpoints[currentSetPointIndex].timeConstS_cool, LocalTimerElapsed2);
                        tempType = "Cooling";
                        TCVM.LocalTimer2.Reset();
                    }
                }
                await Task.Run(() =>
                {
                    try
                    {
                        ReadTemperature();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                });
                TCVM.MainPlot.Plot.Axes.AutoScale();
                TCVM.MainPlot.Refresh();
                TCVM.LocalTimer.Reset();
                TCVM.LocalTimer.Start();
            }
        }

        private void ipTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string ip = ipTextBox.Text;
            bool isValid = IsStrictIPv4(ip);
            ipTextBox.BorderBrush = isValid ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
            ipTextBox.Foreground = isValid ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
            ConnectButt.IsEnabled = isValid;
        }

        private void IntervalChangeButt_Click(object sender, RoutedEventArgs e)
        {
            TCVM.Interval = intResult;
            TCVM.LocalTimer.Stop();
            TCVM.LocalTimer = new LocalTimer(intResult, LocalTimerElapsed);
            TCVM.LocalTimer.Reset();
            TCVM.LocalTimer.Start();
        }
    }
}
