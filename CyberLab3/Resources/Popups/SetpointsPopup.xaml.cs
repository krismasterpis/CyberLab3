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
using System.Windows.Shapes;

namespace CyberLab3.Resources.Popups
{
    public partial class SetpointsPopup : Window
    {
        // Plottable z naszymi punktami (checkpointami)
        private Scatter MyScatterPlot;

        // Lista etykiet tekstowych, każda przypisana do jednego punktu
        private List<double> MyPointsX;
        private List<double> MyPointsY;
        private List<Text> MyLabels;

        // --- Zmienne do śledzenia stanu przeciągania ---

        // Indeks punktu, który jest aktualnie przeciągany
        private int? draggedPointIndex = null;

        // Specjalny marker do wizualnego podświetlenia przeciąganego punktu
        private Marker highlightedMarker;
        public SetpointsPopup(List<double> inputX_, List<double> inputY_)
        {
            InitializeComponent();
            MyPointsX = inputX_;
            MyPointsY = inputY_;
        }
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            InitializePlot();
        }

        private void InitializePlot()
        {
            MyLabels = new List<Text>();
            MyScatterPlot = WpfPlot1.Plot.Add.Scatter(MyPointsX, MyPointsY);
            WpfPlot1.Plot.FigureBackground.Color = ScottPlot.Color.FromColor(System.Drawing.Color.Transparent);
            PixelPadding padding = new PixelPadding(75, 35, 75, 35);
            WpfPlot1.Plot.Layout.Fixed(padding);
            WpfPlot1.Plot.XLabel("Checkpoints");
            WpfPlot1.Plot.YLabel("Temperature (°C)");
            WpfPlot1.Plot.Axes.Left.Label.FontSize = 20;
            WpfPlot1.Plot.Axes.Left.TickLabelStyle.FontSize = 16;
            WpfPlot1.Plot.Axes.Bottom.Label.FontSize = 20;
            WpfPlot1.Plot.Axes.Bottom.TickLabelStyle.FontSize = 16;
            WpfPlot1.Plot.Axes.Left.Min = -50;
            WpfPlot1.Plot.Axes.Left.Max = 200;
            WpfPlot1.Plot.Axes.Bottom.Min = 0;
            WpfPlot1.Plot.Axes.Bottom.Max = 6;
            MyScatterPlot.LineStyle.Width = 3;
            MyScatterPlot.MarkerStyle.Size = 10;
            MyScatterPlot.MarkerStyle.FillColor = ScottPlot.Color.FromColor(System.Drawing.Color.Blue);
            for (int i = 0; i < MyPointsX.Count(); i++)
            {
                var label = CreateLabelForPoint(MyPointsX[i], MyPointsY[i]);
                MyLabels.Add(label);
                WpfPlot1.Plot.Add.Plottable(label);
            }
            highlightedMarker = WpfPlot1.Plot.Add.Marker(0, 0);
            highlightedMarker.IsVisible = false;
            highlightedMarker.Size = 15;
            highlightedMarker.Color = ScottPlot.Color.FromColor(System.Drawing.Color.Red);
            highlightedMarker.Shape = ScottPlot.MarkerShape.OpenCircle;
            highlightedMarker.LineWidth = 2;
            WpfPlot1.UserInputProcessor.UserActionResponses.Clear();
            WpfPlot1.Refresh();
        }

        private void WpfPlot1_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Pixel mousePixel = new(e.GetPosition(WpfPlot1).X, e.GetPosition(WpfPlot1).Y);
            Coordinates mouseCoordinates = WpfPlot1.Plot.GetCoordinates(mousePixel);
            DataPoint nearestPoint = MyScatterPlot.Data.GetNearest(mouseCoordinates, WpfPlot1.Plot.LastRender, 10);
            if (nearestPoint.IsReal)
            {
                draggedPointIndex = nearestPoint.Index;
                highlightedMarker.Location = nearestPoint.Coordinates;
                highlightedMarker.IsVisible = true;
                WpfPlot1.Refresh();
            }
        }

        private void WpfPlot1_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (draggedPointIndex.HasValue)
            {
                Pixel mousePixel = new(e.GetPosition(WpfPlot1).X, e.GetPosition(WpfPlot1).Y);
                Coordinates mouseCoordinates = WpfPlot1.Plot.GetCoordinates(mousePixel);
                int index = draggedPointIndex.Value;
                if(mouseCoordinates.Y >= -40 && mouseCoordinates.Y <= 180)
                {
                    MyPointsY[index] = mouseCoordinates.Y;
                    double originalX = MyPointsX[index];
                    highlightedMarker.Location = new Coordinates(originalX, mouseCoordinates.Y);
                    UpdateLabel(index);
                }
                else
                {
                    if(mouseCoordinates.Y < -40)
                    {
                        MyPointsY[index] = -40;
                        double originalX = MyPointsX[index];
                        highlightedMarker.Location = new Coordinates(originalX, -40);
                        UpdateLabel(index);
                    }
                    else
                    {
                        MyPointsY[index] = 180;
                        double originalX = MyPointsX[index];
                        highlightedMarker.Location = new Coordinates(originalX, 180);
                        UpdateLabel(index);
                    }
                }    
                WpfPlot1.Refresh();
            }
        }

        private void WpfPlot1_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            draggedPointIndex = null;
            highlightedMarker.IsVisible = false;
            WpfPlot1.Refresh();
        }
        private Text CreateLabelForPoint(double x, double y)
        {
            return new Text()
            {
                LabelText = y.ToString("F1"), // Tekst etykiety, sformatowany do 1 miejsca po przecinku
                Location = new Coordinates(x, y),
                LabelFontSize = 12,
                OffsetX = 5, // Przesunięcie w prawo od punktu
                OffsetY = -5, // Przesunięcie w górę od punktu
                Alignment = Alignment.LowerLeft
            };
        }

        private void UpdateLabel(int index)
        {
            double newY = MyPointsY[index];
            double originalX = MyPointsX[index];
            var labelToUpdate = MyLabels[index];
            labelToUpdate.LabelText = newY.ToString("F1");
            labelToUpdate.Location = new Coordinates(originalX, newY);
        }

        private void WpfPlot1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Pixel mousePixel = new(e.GetPosition(WpfPlot1).X, e.GetPosition(WpfPlot1).Y);
            Coordinates mouseCoordinates = WpfPlot1.Plot.GetCoordinates(mousePixel);
            DataPoint nearestPoint = MyScatterPlot.Data.GetNearest(mouseCoordinates, WpfPlot1.Plot.LastRender, 10);
            if (nearestPoint.IsReal)
            {
                MyPointsY[nearestPoint.Index] = 0;
                double originalX = MyPointsX[nearestPoint.Index];
                highlightedMarker.Location = new Coordinates(originalX, mouseCoordinates.Y);
                UpdateLabel(nearestPoint.Index);
                WpfPlot1.Refresh();
            }
        }
        public List<double> GetPoints()
        {
            return MyPointsY;
        }

        private void checkButt_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void plusButt_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyPointsY.Add(0);
            MyPointsX.Add(MyPointsY.Count());
            WpfPlot1.Plot.Axes.Bottom.Max = MyPointsX.Count()+1;
            var label = CreateLabelForPoint(MyPointsX.Last(), MyPointsY.Last());
            MyLabels.Add(label);
            WpfPlot1.Plot.Add.Plottable(label);
            WpfPlot1.Refresh();
        }

        private void minusButt_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(MyPointsY.Count()>2)
            {
                MyPointsY.RemoveAt(MyPointsY.Count() - 1);
                MyPointsX.RemoveAt(MyPointsX.Count() - 1);
                WpfPlot1.Refresh();
            }
        }
    }
}
