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

namespace CyberLab3.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy OsaPage.xaml
    /// </summary>
    public partial class OsaPage : Page
    {
        OSA osa = new OSA();
        OsaPageViewModel OsaPVM = new OsaPageViewModel();
        List<Measurement> measurements = new List<Measurement>();
        Dictionary<string, Scatter> scatters = new Dictionary<string, Scatter>();
        Dictionary<string, List<double>> scattersXs = new Dictionary<string, List<double>>();
        Dictionary<string, List<double>> scattersYs = new Dictionary<string, List<double>>();
        Dictionary<string, TraceType> tracesTypes = new Dictionary<string, TraceType>();
        private List<System.Drawing.Color> pallette = new List<System.Drawing.Color>() {System.Drawing.Color.Yellow, System.Drawing.Color.Cyan, System.Drawing.Color.Red, System.Drawing.Color.Lime, System.Drawing.Color.Orange, System.Drawing.Color.Fuchsia, System.Drawing.Color.DarkGreen, System.Drawing.Color.DarkViolet, System.Drawing.Color.Brown, System.Drawing.Color.DarkBlue};
        Measurement currentMeasure;
        Measurement prevMeasure;
        ScottPlot.AxisRules.MaximumBoundary normalRule;
        public OsaPage()
        {
            InitializeComponent();
            DataContext = OsaPVM;
            OsaPVM.OSAplot.Plot.FigureBackground.Color = ScottPlot.Color.FromColor(System.Drawing.Color.Transparent);
            PixelPadding padding = new PixelPadding(75, 35, 75, 35);
            OsaPVM.OSAplot.Plot.Layout.Fixed(padding);
            OsaPVM.OSAplot.Plot.Axes.Left.Label.Text = "Vertical Axis";
            OsaPVM.OSAplot.Plot.Axes.Left.Label.FontSize = 20;
            OsaPVM.OSAplot.Plot.Axes.Left.TickLabelStyle.FontSize = 16;
            OsaPVM.OSAplot.Plot.Axes.Bottom.Label.Text = "Horizontal Axis";
            OsaPVM.OSAplot.Plot.Axes.Bottom.Label.FontSize = 20;
            OsaPVM.OSAplot.Plot.Axes.Bottom.TickLabelStyle.FontSize = 16;
            normalRule = new(xAxis: OsaPVM.OSAplot.Plot.Axes.Bottom,
                                                           yAxis: OsaPVM.OSAplot.Plot.Axes.Left,
                                                           new ScottPlot.AxisLimits(OSA.WAVELENGTH_RANGE_MIN_nm, OSA.WAVELENGTH_RANGE_MAX_nm, OSA.RECEIVER_SENSITIVITY_dBm, OSA.MAX_OPTICAL_INPUT_LEVEL_dBm));
            OsaPVM.OSAplot.Plot.Axes.Rules.Clear();
            OsaPVM.OSAplot.Plot.Axes.Rules.Add(normalRule);
            currentMeasure = new Measurement();
            for (int i = 0; i < 10; i++)
            {
                var name = $"Trace({(char)('A' + i)})";
                tracesTypes.Add(name, TraceType.Blank);
                scattersXs.Add(name,new List<double>());
                scattersYs.Add(name,new List<double>());
                var scatter = OsaPVM.OSAplot.Plot.Add.Scatter(scattersXs[name], scattersYs[name]);
                scatter.Color = ScottPlot.Color.FromColor(pallette[i]);
                scatter.LegendText = name;
                scatter.IsVisible = false;
                scatters.Add(name, scatter);
            }
            OsaPVM.OSAplot.Refresh();
            OsaPVM.IpAddress = "172.16.2.80";
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                osa.Connect($"TCPIP0::{OsaPVM.IpAddress.Trim()}::inst0::INSTR");

                if (osa.IsConnected)
                {
                    string[] str = osa.Query(AnritsuMs9740AScpiCommands.IDENTIFICATION_QUERY).Split(',');
                    osa.identity = str[0] + " " + str[1];
                }
            });
            DeviceTextBlock.Text = osa.identity;
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
            OsaPVM.OSAplot.Refresh();
            OsaPVM.OSAplot.Plot.Axes.AutoScale();
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
                OsaPVM.OSAplot.Plot.Axes.Rules.Clear();
                OsaPVM.OSAplot.Refresh();
            }
            else
            {
                OsaPVM.OSAplot.Plot.Axes.Rules.Clear();
                OsaPVM.OSAplot.Plot.Axes.Rules.Add(normalRule);
                OsaPVM.OSAplot.Refresh();
                OsaPVM.OSAplot.Plot.Axes.AutoScale();
            }
        }
    }
}