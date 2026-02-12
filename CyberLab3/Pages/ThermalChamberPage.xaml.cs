using CyberLab3.Resources.Controls;
using CyberLab3.Resources.Libraries;
using CyberLab3.Resources.Popups;
using CyberLab3.Resources.Services;
using HarfBuzzSharp;
using Microsoft.Win32;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.TickGenerators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
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
using static ScottPlot.Generate;

namespace CyberLab3.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy ThermalChamberPage.xaml
    /// </summary>
    public partial class ThermalChamberPage : Page
    {
        private static readonly Regex rgx = new Regex("[0-9]+");
        private readonly TimerEventService _timerService;
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

        private SemaphoreSlim _sem1 = new SemaphoreSlim(1, 1);

        int currentSetPointIndex = 0;
        int localSetPoint = -100;
        int intResult;
        string tempType = "Idle";
        bool measurementFailed = false;
        bool measurementEnabled = false;

        string defaultAutosaveFolderPath = "./temperature_autosave";
        public ThermalChamberPage(ThermalChamberViewModel _VM, OsaPageViewModel _OPVM)
        {
            InitializeComponent();
            TCVM = _VM;
            DataContext = TCVM;
            _timerService = ((App)Application.Current).TimerService;
            TCVM.MainPlot.Plot.FigureBackground.Color = ScottPlot.Color.FromColor(System.Drawing.Color.Transparent);
            TCVM.SetPointsPlot.Plot.FigureBackground.Color = ScottPlot.Color.FromColor(System.Drawing.Color.Transparent);
            PixelPadding padding = new PixelPadding(75, 35, 75, 35);
            TCVM.MainPlot.Plot.Layout.Fixed(padding);
            TCVM.SetPointsPlot.Plot.Layout.Fixed(padding);
            TCVM.MainPlot.Plot.Axes.Left.Label.Text = "Temperature (°C)";
            TCVM.SetPointsPlot.Plot.Axes.Left.Label.Text = "Temperature (°C)";
            TCVM.MainPlot.Plot.Axes.Left.Label.FontSize = 20;
            TCVM.SetPointsPlot.Plot.Axes.Left.Label.FontSize = 20;
            TCVM.MainPlot.Plot.Axes.Left.TickLabelStyle.FontSize = 16;
            TCVM.SetPointsPlot.Plot.Axes.Left.TickLabelStyle.FontSize = 16;
            TCVM.MainPlot.Plot.Axes.Bottom.Label.Text = "Time (s)";
            TCVM.SetPointsPlot.Plot.Axes.Bottom.Label.Text = "Setpoints (-)";
            TCVM.MainPlot.Plot.Axes.Bottom.Label.FontSize = 20;
            TCVM.SetPointsPlot.Plot.Axes.Bottom.Label.FontSize = 20;
            TCVM.MainPlot.Plot.Axes.Bottom.TickLabelStyle.FontSize = 16;
            TCVM.SetPointsPlot.Plot.Axes.Bottom.TickLabelStyle.FontSize = 16;
            TCVM.SetPointsPlot.UserInputProcessor.UserActionResponses.Clear();
            setpointsScatter = TCVM.SetPointsPlot.Plot.Add.Scatter(temperatureSetpointsX, temperatureSetpointsY);
            setpointsScatter.LineWidth = 3;
            setpointsScatter.MarkerStyle.Size = 10;
            setpointsScatter.MarkerStyle.FillColor = ScottPlot.Color.FromColor(System.Drawing.Color.Blue);
            TCVM.SetPointsPlot.Plot.Axes.Bottom.TickGenerator = new NumericFixedInterval(1);
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
            TCVM.LocalTimer3 = new LocalTimer(300, LocalTimer3Elapsed);
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
                        var counter = 0;
                        var status = true;
                        while(counter < 10 && status)
                        {
                            try
                            {
                                counter += 1;
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
            if (TCVM.LocalTimer == null) return;
            
            _ = RunMeasurementCycleAsync();
        }
        private async Task RunMeasurementCycleAsync()
        {
            if (!await _sem1.WaitAsync(0))
            {
                Debug.WriteLine("Poprzedni pomiar trwa zbyt długo (powyżej 1s). Pomijam ten cykl.");
                return;
            }

            try
            {
                await Task.Run(async () =>
                {
                    var localSw = Stopwatch.StartNew();

                    try
                    {
                        ReadTemperature();
                        if (!TC.checkIfConnected()) TC.Connect();
                        measurementFailed = false;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        measurementFailed = true;
                    }

                    // --- Twoja pętla retry (może trwać np. 300-400ms) ---
                    int count = 0;
                    while (measurementFailed && count < 3)
                    {
                        try
                        {
                            if (!TC.checkIfConnected())
                                TC.Connect();
                            else
                                ReadTemperature();

                            measurementFailed = false;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message + $" Count:{count}");
                            measurementFailed = true;
                        }

                        count++;
                        await Task.Delay(100); // To opóźnienie jest bezpieczne wewnątrz Task.Run
                    }
                    // -----------------------------------------------------

                    if (measurementFailed)
                    {
                        if (temperatureX.Any())
                        {
                            errorX.Add(temperatureX.Last());
                            errorY.Add(temperatureY.Last());
                        }
                        TCVM.MeasurementFails += 1;
                        measurementFailed = false;
                    }

                    localSw.Stop();

                    // Logika Setpointów i Tau
                    if (measurementEnabled && temperatureSetpoints.Any() && currentSetPointIndex < temperatureSetpoints.Count)
                    {
                        bool coolingOk = tempType == "Cooling" && TCVM.CurrTemperature <= tauLine.Y;
                        bool heatingOk = tempType == "Heating" && TCVM.CurrTemperature >= tauLine.Y;
                        bool rangeOk = TCVM.CurrTemperature > (TCVM.CurrSetPoint - 0.4) && TCVM.CurrTemperature < (TCVM.CurrSetPoint + 0.4);

                        if (coolingOk || heatingOk || rangeOk)
                        {
                            TCVM.LocalTimer2.Start();
                        }
                    }

                    elapsedMSList.Add((int)localSw.ElapsedMilliseconds);
                    TCVM.ElapsedMsAvg = elapsedMSList.Average();
                    TCVM.ElapsedMs = localSw.ElapsedMilliseconds;
                });
                Dispatcher.Invoke(() =>
                {
                    TCVM.MainPlot.Plot.Axes.AutoScale();
                    TCVM.MainPlot.Refresh();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Krytyczny błąd w pętli pomiarowej: {ex.Message}");
            }
            finally
            {
                _sem1.Release();
            }
        }
        private async void LocalTimerElapsed2(object? sender, EventArgs e)
        {
            if (TCVM.LocalTimer != null)
            {
                if (currentSetPointIndex < temperatureSetpoints.Count() - 1)
                {
                    currentSetPointIndex += 1;
                    _timerService.RaiseTimerEvent();
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
                    var tau = temperatureSetpoints[currentSetPointIndex - 1].temperature + (temperatureSetpoints[currentSetPointIndex].temperature - temperatureSetpoints[currentSetPointIndex - 1].temperature) * 0.63;
                    if (temperatureSetpoints[currentSetPointIndex].timeS > 0)
                    {
                        TCVM.LocalTimer2.Stop();
                        if(temperatureSetpoints[currentSetPointIndex].timeS == -1)
                        {
                            TCVM.LocalTimer2 = new LocalTimer(0, LocalTimerElapsed2);
                        }
                        else
                        {
                            TCVM.LocalTimer2 = new LocalTimer(temperatureSetpoints[currentSetPointIndex].timeS, LocalTimerElapsed2);
                        }
                        if (temperatureSetpoints[currentSetPointIndex - 1].temperature <= temperatureSetpoints[currentSetPointIndex].temperature)
                        {
                            tempType = "Heating";
                        }
                        else
                        {
                            tempType = "Cooling";
                        }
                        TCVM.LocalTimer2.Reset();
                    }
                    else
                    {
                        if (temperatureSetpoints[currentSetPointIndex].timeConstS_cool > 0 || temperatureSetpoints[currentSetPointIndex].timeConstS_heat > 0)
                        {
                            if (temperatureSetpoints[currentSetPointIndex - 1].temperature <= temperatureSetpoints[currentSetPointIndex].temperature)
                            {
                                TCVM.LocalTimer2.Stop();
                                TCVM.LocalTimer2 = new LocalTimer(temperatureSetpoints[currentSetPointIndex].timeConstS_heat, LocalTimerElapsed2);
                                tempType = "Heating";
                                TCVM.LocalTimer2.Reset();
                            }
                            else
                            {
                                TCVM.LocalTimer2.Stop();
                                TCVM.LocalTimer2 = new LocalTimer(temperatureSetpoints[currentSetPointIndex].timeConstS_cool, LocalTimerElapsed2);
                                tempType = "Cooling";
                                TCVM.LocalTimer2.Reset();
                            }
                        }
                    }
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
                }
                else
                {
                    TCVM.LocalTimer2.Stop();
                    measurementEnabled = false;
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
                var tau = TCVM.CurrTemperature + (temperatureSetpoints[currentSetPointIndex].temperature - TCVM.CurrTemperature) * 0.63;
                if (temperatureSetpoints[currentSetPointIndex].timeS > 0)
                {
                    if (TCVM.LocalTimer2 != null) TCVM.LocalTimer2.Stop();
                    TCVM.LocalTimer2 = new LocalTimer(temperatureSetpoints[currentSetPointIndex].timeS, LocalTimerElapsed2);
                    if (TCVM.CurrTemperature <= temperatureSetpoints[currentSetPointIndex].temperature)
                    {
                        tempType = "Heating";
                    }
                    else
                    {
                        tempType = "Cooling";
                    }
                    //TCVM.LocalTimer2.Reset();
                    //TCVM.LocalTimer2.Start();
                }
                else if (temperatureSetpoints[currentSetPointIndex].timeConstS_cool > 0 || temperatureSetpoints[currentSetPointIndex].timeConstS_heat > 0)
                {
                    if (TCVM.CurrTemperature <= temperatureSetpoints[currentSetPointIndex].temperature)
                    {
                        if (TCVM.LocalTimer2 != null) TCVM.LocalTimer2.Stop();
                        TCVM.LocalTimer2 = new LocalTimer(temperatureSetpoints[currentSetPointIndex].timeConstS_heat, LocalTimerElapsed2);
                        tempType = "Heating";
                        //TCVM.LocalTimer2.Reset();
                    }
                    else
                    {
                        if (TCVM.LocalTimer2 != null) TCVM.LocalTimer2.Stop();
                        TCVM.LocalTimer2 = new LocalTimer(temperatureSetpoints[currentSetPointIndex].timeConstS_cool, LocalTimerElapsed2);
                        tempType = "Cooling";
                        //TCVM.LocalTimer2.Reset();
                    }
                }
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
                measurementEnabled = true;
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

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            temperatureX.Clear();
            temperatureY.Clear();
            errorX.Clear();
            errorY.Clear();
            TCVM.MainPlot.Refresh();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            dialog.Title = "Choose destination folder";
            var dateTime = System.DateTime.Now;
            List<float> tempX = new List<float>(temperatureX);
            List<float> tempY = new List<float>(temperatureY);
            if (dialog.ShowDialog() == true)
            {
                string folderPath = dialog.FolderName;
                if(folderPath != null && folderPath != string.Empty)
                {
                    var csv = new StringBuilder();
                    string newLine = string.Empty;
                    newLine = $"Time (s); Temperature (°C)";
                    csv.AppendLine(newLine);
                    List<string> stringList = new List<string>();
                    for (int i = 0; i < tempX.Count(); i++)
                    {
                        newLine = string.Empty;
                        stringList.Clear();
                        stringList.Add(tempX[i].ToString());
                        stringList.Add(tempY[i].ToString());
                        newLine = String.Join(';', stringList);
                        csv.AppendLine(newLine);
                    }
                    var FileName = $"{folderPath}/Temperature-{dateTime.Day}-{dateTime.Month}-{dateTime.Year}_{dateTime.Hour}-{dateTime.Minute}-{dateTime.Second}.csv";
                    File.WriteAllText(FileName, csv.ToString());
                }
            }
        }
        private async void LocalTimer3Elapsed(object? sender, EventArgs e)
        {
            if (TCVM.LocalTimer3 != null)
            {
                List<float> tempX = new List<float>(temperatureX);
                List<float> tempY = new List<float>(temperatureY);
                await Task.Run(() =>
                {
                    var dateTime = System.DateTime.Now;
                    var csv = new StringBuilder();
                    string newLine = string.Empty;
                    newLine = $"Time (s); Temperature (°C)";
                    csv.AppendLine(newLine);
                    List<string> stringList = new List<string>();
                    for (int i = 0; i < tempX.Count(); i++)
                    {
                        newLine = string.Empty;
                        stringList.Clear();
                        stringList.Add(tempX[i].ToString());
                        stringList.Add(tempY[i].ToString());
                        newLine = String.Join(';', stringList);
                        csv.AppendLine(newLine);
                    }
                    if(!Directory.Exists(defaultAutosaveFolderPath))
                    {
                        Directory.CreateDirectory(defaultAutosaveFolderPath);
                    }
                    var FileName = defaultAutosaveFolderPath +$"/Temperature-{dateTime.Day}-{dateTime.Month}-{dateTime.Year}_{dateTime.Hour}-{dateTime.Minute}-{dateTime.Second}.csv";
                    File.WriteAllText(FileName, csv.ToString());
                });
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void AutosaveCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(TCVM.IsAutosaveEnabled)
            {
                TCVM.LocalTimer3.Start();
                AutosaveTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                TCVM.LocalTimer3.Stop();
                AutosaveTextBlock.Visibility = Visibility.Hidden;
            }
        }
    }
}
