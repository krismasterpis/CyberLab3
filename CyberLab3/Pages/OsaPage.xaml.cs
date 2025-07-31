using CyberLab3.Resources.Controls;
using CyberLab3.Resources.Libraries;
using CyberLab3.Resources.Popups;
using Ivi.Visa;
using IviVisaNetSample;
using Microsoft.Extensions.Logging;
using NationalInstruments.Visa;
using ScottPlot;
using ScottPlot.Plottables;
using ScpiNet;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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
        public OsaPage(OsaPageViewModel _VM)
        {
            InitializeComponent();
            OPVM = _VM;
            DataContext = OPVM;
            OPVM.OSAplot.Plot.FigureBackground.Color = ScottPlot.Color.FromColor(System.Drawing.Color.Transparent);
            PixelPadding padding = new PixelPadding(75, 35, 75, 35);
            OPVM.OSAplot.Plot.Layout.Fixed(padding);
            OPVM.OSAplot.Plot.Axes.Left.Label.Text = "Vertical Axis";
            OPVM.OSAplot.Plot.Axes.Left.Label.FontSize = 20;
            OPVM.OSAplot.Plot.Axes.Left.TickLabelStyle.FontSize = 16;
            OPVM.OSAplot.Plot.Axes.Bottom.Label.Text = "Horizontal Axis";
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
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                osa.Connect($"TCPIP0::{OPVM.IpAddress.Trim()}::inst0::INSTR");

                if (osa.IsConnected)
                {
                    string[] str = osa.Query(AnritsuMs9740AScpiCommands.IDENTIFICATION_QUERY).Split(',');
                    osa.identity = str[0] + " " + str[1];
                }
            });
            DeviceTextBlock.Text = osa.identity;
            OPVM.LimitStr = $">{AnritsuMs9740ATimeConsts.VBWcoeff[(int)osa.VBW_SETTING] * AnritsuMs9740ATimeConsts.SamplingPointsCoeff[osa.SAMPLING_POINTS_COUNT]}";
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
                osa.ReadSingle(currentMeasure, tracesTypes);
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

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            if(!IntervalTextBox.IsEnabled)
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

        private async void LocalTimerElapsed(object? sender, EventArgs e)
        {
            if(OPVM.LocalTimer != null)
            {
                stopWatch.Restart();
                prevMeasure = currentMeasure;
                await Task.Run(() =>
                {
                    osa.ReadSingle(currentMeasure, tracesTypes);
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
                OPVM.LocalMeasNum += 1;
            }
        }

        private void IntervalTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var result = !rgx.IsMatch(e.Text);
            e.Handled = result;
        }
    }
}