using CyberLab3.Resources.Libraries;
using ScottPlot;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
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
    /// Logika interakcji dla klasy TemperaturePage.xaml
    /// </summary>
    public partial class TemperaturePage : Page
    {
        TemperaturePageViewModel TPVM;
        ScottPlot.AxisRules.MaximumBoundary normalRule;
        public TemperaturePage(TemperaturePageViewModel _VM)
        {
            InitializeComponent();
            TPVM = _VM;
            DataContext = TPVM;

            TPVM.TempPlot.Plot.FigureBackground.Color = ScottPlot.Color.FromColor(System.Drawing.Color.Transparent);
            PixelPadding padding = new PixelPadding(75, 35, 75, 35);
            TPVM.TempPlot.Plot.Layout.Fixed(padding);
            TPVM.TempPlot.Plot.Axes.Left.Label.Text = "Vertical Axis";
            TPVM.TempPlot.Plot.Axes.Left.Label.FontSize = 20;
            TPVM.TempPlot.Plot.Axes.Left.TickLabelStyle.FontSize = 16;
            TPVM.TempPlot.Plot.Axes.Bottom.Label.Text = "Horizontal Axis";
            TPVM.TempPlot.Plot.Axes.Bottom.Label.FontSize = 20;
            TPVM.TempPlot.Plot.Axes.Bottom.TickLabelStyle.FontSize = 16;
            TPVM.TempPlot.Plot.Axes.Rules.Clear();
            TPVM.TempPlot.Refresh();
        }
    }
}
