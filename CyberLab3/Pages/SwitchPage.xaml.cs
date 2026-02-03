using CyberLab3.Resources.Libraries;
using CyberLab3.Resources.Nodes;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// Logika interakcji dla klasy SwitchPage.xaml
    /// </summary>
    public partial class SwitchPage : Page
    {
        SwitchPageViewModel SPVM;
        EXFO_LTB8 LTB8;

        private int _currentSelectionIndex = -1;
        private Path _currentConnectionPath;

        public SwitchPage(SwitchPageViewModel _VM)
        {
            InitializeComponent();
            SPVM = _VM;
            DataContext = SPVM;

            GenerateOutputs(8);

            // Draw the initial line once the UI is fully loaded and measured
            this.Loaded += (s, e) => SelectOutput(_currentSelectionIndex);

            // Redraw line if window resizes
            this.SizeChanged += (s, e) => DrawConnection(_currentSelectionIndex);
        }

        private void ConnectButt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LTB8 = new EXFO_LTB8(IpTextBox.Text.ToString());
                LTB8.Connect();
                DeviceList.ItemsSource = LTB8.InstrumentCatalog;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateOutputs(int count)
        {
            OutputStackPanel.Children.Clear();

            for (int i = 1; i <= count+1; i++)
            {
                if (i == count / 2 + 1)
                {
                    // Create the visual box for output
                    var border = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(120, 120, 120)),
                        CornerRadius = new CornerRadius(5),
                        Padding = new Thickness(15, 10, 15, 10),
                        Margin = new Thickness(0, 5, 0, 5), // Margin between items
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        Cursor = System.Windows.Input.Cursors.Hand,
                        Tag = -1 // Store index in Tag
                    };

                    var text = new TextBlock
                    {
                        Text = $"No continuity",
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.SemiBold,
                        FontSize = 32,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(20, 10, 20, 10)
                    };

                    border.Child = text;

                    // Add click event to change selection
                    border.MouseLeftButtonDown += (s, e) =>
                    {
                        int index = (int)((Border)s).Tag;
                        SelectOutput(index);
                    };
                    OutputStackPanel.Children.Add(border);
                }
                else
                {
                    // Create the visual box for output
                    var border = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                        CornerRadius = new CornerRadius(5),
                        Padding = new Thickness(15, 10, 15, 10),
                        Margin = new Thickness(0, 5, 0, 5), // Margin between items
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        Cursor = System.Windows.Input.Cursors.Hand,
                        Tag = i - 1 // Store index in Tag
                    };

                    var text = new TextBlock
                    {
                        Text = $"Output {i}",
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.SemiBold,
                        FontSize = 32,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(20, 10, 20, 10)
                    };

                    border.Child = text;

                    // Add click event to change selection
                    border.MouseLeftButtonDown += (s, e) =>
                    {
                        int index = (int)((Border)s).Tag;
                        if(index != _currentSelectionIndex) SelectOutput(index);
                    };
                    OutputStackPanel.Children.Add(border);
                }
            }
        }

        public void SelectOutput(int index)
        {
            if (index < -1 || index >= OutputStackPanel.Children.Count) return;

            // 1. Visual update for boxes (Highlight selected, dim others)

            _currentSelectionIndex = index;
            for (int i = 0; i < OutputStackPanel.Children.Count; i++)
            {
                var border = (Border)OutputStackPanel.Children[i];
                if (i == index)
                {
                    border.BorderBrush = new SolidColorBrush(Color.FromRgb(100, 255, 100)); // Bright Green
                    border.BorderThickness = new Thickness(5);
                }
                else
                {
                    border.BorderBrush = Brushes.Gray;
                    border.BorderThickness = new Thickness(1);
                }
            }
            if (index == -1)
            {
                var border = (Border)OutputStackPanel.Children[OutputStackPanel.Children.Count / 2];
                border.BorderBrush = new SolidColorBrush(Color.FromRgb(60, 60, 60)); // Bright Green
                border.BorderThickness = new Thickness(5);
                DrawConnection(OutputStackPanel.Children.Count / 2);
            }
            else
            {
                DrawConnection(index);
            }
        }

        private void DrawConnection(int targetIndex)
        {
            if (targetIndex == -1) return;

            ConnectionCanvas.Children.Clear();

            Point startPoint = GetElementPoint(InputBox, isRightSide: true);

            var targetBorder = (Border)OutputStackPanel.Children[targetIndex];
            Point endPoint = GetElementPoint(targetBorder, isRightSide: false);

            double overlapAmount = 25.0; 

            startPoint.X -= overlapAmount;
            endPoint.X += overlapAmount;
            // -----------------------

            double controlX = (endPoint.X - startPoint.X) / 2;
            Point controlPoint1 = new Point(startPoint.X + controlX, startPoint.Y);
            Point controlPoint2 = new Point(endPoint.X - controlX, endPoint.Y);

            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure { StartPoint = startPoint };
            BezierSegment bezier = new BezierSegment(controlPoint1, controlPoint2, endPoint, true);
            figure.Segments.Add(bezier);
            geometry.Figures.Add(figure);
            var strokeColor = new SolidColorBrush(Color.FromRgb(100, 255, 100));
            if (_currentSelectionIndex == -1) strokeColor = new SolidColorBrush(Color.FromRgb(60, 60, 60));
            _currentConnectionPath = new Path
                {
                    Stroke = strokeColor,
                    StrokeThickness = 10,
                    Data = geometry,
                    SnapsToDevicePixels = true
                };

            ConnectionCanvas.Children.Add(_currentConnectionPath);
        }

        // Helper to get absolute position relative to the Canvas
        private Point GetElementPoint(FrameworkElement element, bool isRightSide)
        {
            Point relativePoint = element.TransformToVisual(ConnectionCanvas)
                                         .Transform(new Point(0, 0));

            double x = isRightSide ? relativePoint.X + element.ActualWidth : relativePoint.X;

            double y = relativePoint.Y + (element.ActualHeight / 2);

            return new Point(x, y);
        }
    }
}
