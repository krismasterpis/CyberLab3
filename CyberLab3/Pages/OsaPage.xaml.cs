using CyberLab3.Resources.Controls;
using CyberLab3.Resources.Libraries;
using CyberLab3.Resources.Popups;
using CyberLab3.Resources.Services;
using HarfBuzzSharp;
using Ivi.Visa;
using IviVisaNetSample;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using NationalInstruments.Visa;
using ScottPlot;
using ScottPlot.Plottables;
using ScpiNet;
using SkiaSharp;
using SqliteWpfApp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace CyberLab3.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy OsaPage.xaml
    /// </summary>
    /// 
    public partial class OsaPage : Page
    {
        private static readonly Regex rgx = new Regex("[0-9]+");
        private readonly TimerEventService _timerService;
        private Stopwatch stopWatch = new Stopwatch();
        OSA osa = new OSA();
        OsaPageViewModel OPVM;
        List<Measurement> measurements = new List<Measurement>();
        Dictionary<string, Scatter> scatters = new Dictionary<string, Scatter>();
        Dictionary<string, List<double>> scattersXs = new Dictionary<string, List<double>>();
        Dictionary<string, List<double>> scattersYs = new Dictionary<string, List<double>>();
        Dictionary<string, TraceType> tracesTypes = new Dictionary<string, TraceType>();
        private List<System.Drawing.Color> pallette = new List<System.Drawing.Color>() {System.Drawing.Color.Yellow, System.Drawing.Color.Cyan, System.Drawing.Color.Red, System.Drawing.Color.Lime, System.Drawing.Color.Orange, System.Drawing.Color.Fuchsia, System.Drawing.Color.DarkGreen, System.Drawing.Color.DarkViolet, System.Drawing.Color.Brown, System.Drawing.Color.DarkBlue};
        Measurement currentMeasure;
        Measurement prevMeasure;
        ScottPlot.AxisRules.MaximumBoundary normalRule;
        bool dataBaseEnabled = false;
        bool cyberLabEnabled = false;
        Progress<int> progressIndicator;
        string csvFolderPath;
        const string defaultOsaFolderPath = "./csv/OSA";

        private CancellationTokenSource _cts;
        private Task _workerTask;
        public bool IsBGTaskRunning => _workerTask != null && !_workerTask.IsCompleted;

        public OsaPage(OsaPageViewModel _VM, DatabaseContext db)
        {
            InitializeComponent();
            csvFolderPath = defaultOsaFolderPath;
            var latest = db.Measurements
                                .OrderByDescending(m => m.Time)
                                .FirstOrDefault();
            progressIndicator = new Progress<int>(value =>
            {
                MeasurementProgressBar.Value = value;
            });
            _timerService = ((App)System.Windows.Application.Current).TimerService;
            _timerService.TimerTriggered += OnTimerTriggered;
            OPVM = _VM;
            DataContext = OPVM;
            OPVM.OSAplot.Plot.FigureBackground.Color = ScottPlot.Color.FromColor(System.Drawing.Color.Transparent);
            PixelPadding padding = new PixelPadding(75, 35, 75, 35);
            OPVM.OSAplot.Plot.Layout.Fixed(padding);
            OPVM.OSAplot.Plot.Axes.Left.Label.Text = "Optical Power (dBm)";
            OPVM.OSAplot.Plot.Axes.Left.Label.FontSize = 20;
            OPVM.OSAplot.Plot.Axes.Left.TickLabelStyle.FontSize = 16;
            OPVM.OSAplot.Plot.Axes.Bottom.Label.Text = "Wavelength (nm)";
            OPVM.OSAplot.Plot.Axes.Bottom.Label.FontSize = 20;
            OPVM.OSAplot.Plot.Axes.Bottom.TickLabelStyle.FontSize = 16;
            normalRule = new(xAxis: OPVM.OSAplot.Plot.Axes.Bottom,
                                                           yAxis: OPVM.OSAplot.Plot.Axes.Left,
                                                           new ScottPlot.AxisLimits(OSA.WAVELENGTH_RANGE_MIN_nm, OSA.WAVELENGTH_RANGE_MAX_nm, OSA.RECEIVER_SENSITIVITY_dBm, OSA.MAX_OPTICAL_INPUT_LEVEL_dBm));
            OPVM.OSAplot.Plot.Axes.Rules.Clear();
            OPVM.OSAplot.Plot.Axes.Rules.Add(normalRule);
            currentMeasure = new Measurement();
            for (int i = 0; i < 10; i++)
            {
                var name = $"Trace({(char)('A' + i)})";
                tracesTypes.Add(name, TraceType.Blank);
                scattersXs.Add(name,new List<double>());
                scattersYs.Add(name,new List<double>());
                var scatter = OPVM.OSAplot.Plot.Add.Scatter(scattersXs[name], scattersYs[name]);
                scatter.Color = ScottPlot.Color.FromColor(pallette[i]);
                scatter.LegendText = name;
                scatter.IsVisible = false;
                scatters.Add(name, scatter);
            }
            OPVM.OSAplot.Refresh();
            OPVM.IpAddress = "172.16.2.80";
            using (var context = new DatabaseContext())
            {
                var connection = context.Database.GetDbConnection();
                connection.Open(); // Otwarcie połączenia
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name LIMIT 1;";
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string tableName = reader.GetString(0);
                            tableComboBox.Items.Add(tableName);
                        }
                    }
                }
            }
            if(tableComboBox.Items.Count>0)
            {
                tableComboBox.SelectedIndex = 0;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if(!osa.IsConnected)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        osa.Connect($"TCPIP0::{OPVM.IpAddress.Trim()}::inst0::INSTR");
                        if (osa.IsConnected)
                        {
                            string[] str = osa.Query(AnritsuMs9740AScpiCommands.IDENTIFICATION_QUERY).Split(',');
                            osa.identity = str[0] + " " + str[1];
                        }
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Connection error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
                if(osa.IsConnected)
                {
                    ConnectButton.Content = "Disconnect";
                    DeviceTextBlock.Text = osa.identity;
                }
            }
            else
            {
                osa.Disconnect();
                if (!osa.IsConnected)
                {
                    osa.identity = "Not Connected";
                    ConnectButton.Content = "Connect";
                    DeviceTextBlock.Text = osa.identity;
                }
            }
        }

        private void ipAddressTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string ip = ipAddressTextBox.Text;
            bool isValid = IsStrictIPv4(ip);
            ipAddressTextBox.BorderBrush = isValid ? Brushes.Green : Brushes.Red;
            ipAddressTextBox.Foreground = isValid ? Brushes.Green : Brushes.Red;
            ConnectButton.IsEnabled = isValid;
        }

        public bool IsStrictIPv4(string ip)
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

        private async void SingleButton_Click(object sender, RoutedEventArgs e)
        {
            stopWatch.Restart();
            prevMeasure = currentMeasure; // do przerobienia na prevMeasure = new Measurement(currentMeasure);
            await Task.Run(() =>
            {
                osa.ReadSingle(currentMeasure, tracesTypes, progressIndicator);
                currentMeasure.Time = DateTime.Now;
            });
            foreach(var trace in currentMeasure.traces.Values)
            {
                scattersXs[trace.name].Clear();
                scattersXs[trace.name].AddRange(trace.wavelengths);
                scattersYs[trace.name].Clear();
                scattersYs[trace.name].AddRange(trace.attenuationsRaw);
            }
            OPVM.OSAplot.Refresh();
            OPVM.OSAplot.Plot.Axes.AutoScale();
            stopWatch.Stop();
            OPVM.ElapsedMs = stopWatch.ElapsedMilliseconds;
            if (OPVM.LocalTimer != null) ElapsedTimeTextBlock.Foreground = OPVM.ElapsedMs < OPVM.LocalTimer.Interval*1000 ? Brushes.Green : Brushes.Red;
            else ElapsedTimeTextBlock.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255,42, 132, 241));
        }

        private void OsaTraceControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OsaTraceControl temp = (OsaTraceControl)sender;
            TraceType type = TraceType.Blank;
            switch(temp.SelectedComboBoxItemString)
            {
                case "Write":
                    type = TraceType.Write;
                    osa.SendTraceType(temp.Name.Last(), type);
                    scatters[$"Trace({temp.Name.Last()})"].IsVisible = true;
                    break;
                case "Fix":
                    type = TraceType.Fix;
                    osa.SendTraceType(temp.Name.Last(), type);
                    scatters[$"Trace({temp.Name.Last()})"].IsVisible = true;
                    break;
                case "Blank":
                    type = TraceType.Blank;
                    osa.SendTraceType(temp.Name.Last(), type);
                    scatters[$"Trace({temp.Name.Last()})"].IsVisible = false;
                    break;
                case "Calculate":
                    type = TraceType.Calculate;
                    osa.SendTraceType(temp.Name.Last(), type);
                    scatters[$"Trace({temp.Name.Last()})"].IsVisible = true;
                    break;
                case null:
                    break;
            }
            currentMeasure.traces[$"Trace({temp.Name.Last()})"].type = type;
            if (tracesTypes.ContainsKey($"Trace({temp.Name.Last()})"))
            {
                tracesTypes[$"Trace({temp.Name.Last()})"] = type;
            }
            else
            {
                tracesTypes.Add($"Trace({temp.Name.Last()})", type);
            }
            if(type == TraceType.Calculate)
            {
                OsaCalcPopup popup = new OsaCalcPopup();
                bool? result = popup.ShowDialog();
                currentMeasure.traces[$"Trace({temp.Name.Last()})"].calculateTrace1 = popup.str[0];
                currentMeasure.traces[$"Trace({temp.Name.Last()})"].calculateSign = popup.str[1];
                currentMeasure.traces[$"Trace({temp.Name.Last()})"].calculateTrace2 = popup.str[2];
                temp.Text = "Trace " + popup.str;
            }
            else
            {
                temp.Text = $"Trace {temp.Name.Last()}";
            }
            if(tracesTypes.Values.Contains(TraceType.Calculate))
            {
                OPVM.OSAplot.Plot.Axes.Rules.Clear();
                OPVM.OSAplot.Refresh();
            }
            else
            {
                OPVM.OSAplot.Plot.Axes.Rules.Clear();
                OPVM.OSAplot.Plot.Axes.Rules.Add(normalRule);
                OPVM.OSAplot.Refresh();
                OPVM.OSAplot.Plot.Axes.AutoScale();
            }
        }
        private void OnTimerTriggered()
        {
            // tu można wstawić dowolny kod np. odświeżenie danych
            Debug.WriteLine("Otrzymano sygnał z Page1!");
        }
        private async void LocalTimerElapsed(object? sender, EventArgs e)
        {
            if(OPVM.LocalTimer != null)
            {
                stopWatch.Restart();
                prevMeasure = currentMeasure;
                await Task.Run(() =>
                {
                    currentMeasure.Time = DateTime.Now;
                    osa.ReadSingle(currentMeasure, tracesTypes, progressIndicator);
                    if(App.isThermalChamberActive)
                    {
                        currentMeasure.Temperature = App.globalTemperatureTC;
                    }
                    currentMeasure.Time = DateTime.Now;
                });
                foreach (var trace in currentMeasure.traces.Values)
                {
                    scattersXs[trace.name].Clear();
                    scattersXs[trace.name].AddRange(trace.wavelengths);
                    scattersYs[trace.name].Clear();
                    scattersYs[trace.name].AddRange(trace.attenuationsRaw);
                }
                OPVM.OSAplot.Refresh();
                OPVM.OSAplot.Plot.Axes.AutoScale();
                stopWatch.Stop();
                OPVM.ElapsedMs = stopWatch.ElapsedMilliseconds;
                ElapsedTimeTextBlock.Foreground = OPVM.ElapsedMs < OPVM.LocalTimer.Interval*1000 ? Brushes.Green : Brushes.Red;
                if(cyberLabEnabled)
                {
                    if (dataBaseEnabled)
                    {
                        using (var context = new DatabaseContext())
                        {
                            var latest = context.Measurements
                                .OrderByDescending(m => m.Id)
                                .FirstOrDefault();
                            if (latest.Id == currentMeasure.Id) currentMeasure.Id += 1;
                            context.Measurements.Add(currentMeasure);
                            context.SaveChanges();
                        }
                    }
                    else
                    {
                        saveToCSV();
                    }
                }
                OPVM.LocalMeasNum += 1;
            }
        }

        private void IntervalTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var result = !rgx.IsMatch(e.Text);
            e.Handled = result;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            saveToCSV();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            OsaSettingsPopup popup = new OsaSettingsPopup(osa);
            bool? result = popup.ShowDialog();
        }

        private void SaveComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SaveComboBox.SelectedItem != null && tableSelectionGroup != null)
            {
                if (((ComboBoxItem)SaveComboBox.SelectedItem)?.Content.ToString() == "database table")
                {
                    tableSelectionGroup.IsEnabled = true;
                }
                else
                {
                    tableSelectionGroup.IsEnabled = false;
                }
            }   
        }

        private void saveToCSV()
        {
            if(csvFolderPath != defaultOsaFolderPath && Directory.Exists(csvFolderPath))
            {
                var csv = new StringBuilder();
                string newLine = string.Empty;
                newLine = $"Thermal Chamber Temperature; {currentMeasure.Temperature}";
                csv.AppendLine(newLine);
                List<string> stringList = new List<string>();
                newLine = string.Empty;
                stringList.Clear();
                stringList.Add("Wavelength (nm)");
                foreach (var trace in currentMeasure.traces.Values)
                {
                    if (trace.type != TraceType.Blank)
                    {
                        stringList.Add(trace.name);
                    }
                }
                newLine = String.Join(';', stringList);
                csv.AppendLine(newLine);
                for (int j = 0; j < currentMeasure.traces.First().Value.wavelengths.Count(); j++) //trace iteration
                {
                    newLine = string.Empty;
                    stringList.Clear();
                    stringList.Add(currentMeasure.traces.First().Value.wavelengths[j].ToString());
                    foreach (var trace in currentMeasure.traces.Values)
                    {
                        if (trace.type != TraceType.Blank)
                        {
                            stringList.Add(trace.attenuationsRaw[j].ToString());
                        }
                    }
                    newLine = String.Join(';', stringList);
                    csv.AppendLine(newLine);
                }
                var FileName = $"{csvFolderPath}/OSA-{currentMeasure.Time.Day}-{currentMeasure.Time.Month}-{currentMeasure.Time.Year}_{currentMeasure.Time.Hour}-{currentMeasure.Time.Minute}-{currentMeasure.Time.Second}.csv";
                File.WriteAllText(FileName, csv.ToString());
            }
            else
            {
                if (Directory.Exists(defaultOsaFolderPath))
                {
                    var csv = new StringBuilder();
                    string newLine = string.Empty;
                    newLine = $"Thermal Chamber Temperature; {currentMeasure.Temperature}";
                    csv.AppendLine(newLine);
                    List<string> stringList = new List<string>();
                    newLine = string.Empty;
                    stringList.Clear();
                    stringList.Add("Wavelength (nm)");
                    foreach (var trace in currentMeasure.traces.Values)
                    {
                        if (trace.type != TraceType.Blank)
                        {
                            stringList.Add(trace.name);
                        }
                    }
                    newLine = String.Join(';', stringList);
                    csv.AppendLine(newLine);
                    for (int j = 0; j < currentMeasure.traces.First().Value.wavelengths.Count(); j++) //trace iteration
                    {
                        newLine = string.Empty;
                        stringList.Clear();
                        stringList.Add(currentMeasure.traces.First().Value.wavelengths[j].ToString());
                        foreach (var trace in currentMeasure.traces.Values)
                        {
                            if (trace.type != TraceType.Blank)
                            {
                                stringList.Add(trace.attenuationsRaw[j].ToString());
                            }
                        }
                        newLine = String.Join(';', stringList);
                        csv.AppendLine(newLine);
                    }
                    var FileName = $"{defaultOsaFolderPath}/OSA-{currentMeasure.Time.Day}-{currentMeasure.Time.Month}-{currentMeasure.Time.Year}_{currentMeasure.Time.Hour}-{currentMeasure.Time.Minute}-{currentMeasure.Time.Second}.csv";
                    File.WriteAllText(FileName, csv.ToString());
                }
                else
                {
                    Directory.CreateDirectory(defaultOsaFolderPath);
                    var csv = new StringBuilder();
                    string newLine = string.Empty;
                    newLine = $"Thermal Chamber Temperature; {currentMeasure.Temperature}";
                    csv.AppendLine(newLine);
                    List<string> stringList = new List<string>();
                    newLine = string.Empty;
                    stringList.Clear();
                    stringList.Add("Wavelength (nm)");
                    foreach (var trace in currentMeasure.traces.Values)
                    {
                        if (trace.type != TraceType.Blank)
                        {
                            stringList.Add(trace.name);
                        }
                    }
                    newLine = String.Join(';', stringList);
                    csv.AppendLine(newLine);
                    for (int j = 0; j < currentMeasure.traces.First().Value.wavelengths.Count(); j++) //trace iteration
                    {
                        newLine = string.Empty;
                        stringList.Clear();
                        stringList.Add(currentMeasure.traces.First().Value.wavelengths[j].ToString());
                        foreach (var trace in currentMeasure.traces.Values)
                        {
                            if (trace.type != TraceType.Blank)
                            {
                                stringList.Add(trace.attenuationsRaw[j].ToString());
                            }
                        }
                        newLine = String.Join(';', stringList);
                        csv.AppendLine(newLine);
                    }
                    var FileName = $"{defaultOsaFolderPath}/OSA-{currentMeasure.Time.Day}-{currentMeasure.Time.Month}-{currentMeasure.Time.Year}_{currentMeasure.Time.Hour}-{currentMeasure.Time.Minute}-{currentMeasure.Time.Second}.csv";
                    File.WriteAllText(FileName, csv.ToString());
                }
            }
        }

        private void saveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog
            {
                Title = "Select Folder",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            if (folderDialog.ShowDialog() == true)
            {
                var folderName = folderDialog.FolderName;
                csvFolderPathTextBlock.Text = folderName;
            }
        }

        private void LoopButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IntervalTextBox.IsEnabled)
            {
                OPVM.LocalTimer.Stop();
                IntervalTextBox.IsEnabled = true;
            }
            else
            {
                var interv = long.Parse(IntervalTextBox.Text);
                var limit = AnritsuMs9740ATimeConsts.VBWcoeff[(int)osa.VBW_SETTING] * AnritsuMs9740ATimeConsts.SamplingPointsCoeff[osa.SAMPLING_POINTS_COUNT];
                if (interv > 0 && interv > limit)
                {
                    OPVM.LocalTimer = new LocalTimer(interv, LocalTimerElapsed);
                    OPVM.LocalTimer.Reset();
                    OPVM.LocalTimer.Start();
                    IntervalTextBox.IsEnabled = false;
                }
                else
                {
                    IntervalTextBox.Text = null;
                }
            }
        }

        private void LiveButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsBGTaskRunning) return; // Zabezpieczenie przed podwójnym startem

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            // Uruchomienie zadania na osobny wątku (ThreadPool)
            _workerTask = Task.Run(async () =>
            {
                await BackgroundLoop(token);
            }, token);
        }

        private async Task BackgroundLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    // 1. Wykonaj swoją funkcję
                    stopWatch.Restart();
                    prevMeasure = currentMeasure; // do przerobienia na prevMeasure = new Measurement(currentMeasure);
                    osa.ReadSingle(currentMeasure, tracesTypes, progressIndicator);
                    currentMeasure.Time = DateTime.Now;
                    foreach (var trace in currentMeasure.traces.Values)
                    {
                        scattersXs[trace.name].Clear();
                        scattersXs[trace.name].AddRange(trace.wavelengths);
                        scattersYs[trace.name].Clear();
                        scattersYs[trace.name].AddRange(trace.attenuationsRaw);
                    }
                    this.Dispatcher.InvokeAsync(() =>
                    {
                        OPVM.OSAplot.Refresh();
                        OPVM.OSAplot.Plot.Axes.AutoScale();
                        stopWatch.Stop();
                        OPVM.ElapsedMs = stopWatch.ElapsedMilliseconds;
                        if (OPVM.LocalTimer != null) ElapsedTimeTextBlock.Foreground = OPVM.ElapsedMs < OPVM.LocalTimer.Interval * 1000 ? Brushes.Green : Brushes.Red;
                        else ElapsedTimeTextBlock.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 42, 132, 241));
                    });
                    // 2. Odczekaj określony czas (np. 1 sekunda)
                    // Używamy Task.Delay zamiast Thread.Sleep, żeby nie blokować wątku
                    await Task.Delay(1000, token);
                }
            }
            catch (TaskCanceledException)
            {
                // Normalne zakończenie przy anulowaniu (Stop)
            }
            catch (Exception ex)
            {
                // Logowanie błędów, żeby wątek nie "zginął po cichu"
                Debug.WriteLine($"Błąd w pętli tła: {ex.Message}");
            }
        }
    }
}