using CyberLab3.Resources.Controls;
using CyberLab3.Resources.HomeShorts;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CyberLab3.Pages
{
    public partial class HomePage : Page
    {
        HomePageViewModel HPVM;
        string[] frames = new string[4] {string.Empty, string.Empty , string.Empty , string.Empty };
        public HomePage(HomePageViewModel _VM)
        {
            InitializeComponent();
            HPVM = _VM;
            DataContext = HPVM;
        }

        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && IconTilesListBox.SelectedItem is IconTile item)
            {
                var data = new DataObject(typeof(IconTile), item);
                DragDrop.DoDragDrop(IconTilesListBox, data, DragDropEffects.Move);
            }
        }

        private static void UpdateDragEffects(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(IconTile)))
                e.Effects = DragDropEffects.Move;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        private void DropSurface_DragEnter(object sender, DragEventArgs e) => UpdateDragEffects(e);
        private void DropSurface_DragOver(object sender, DragEventArgs e) => UpdateDragEffects(e);

        private void DropSurface_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(IconTile))) return;
            var tile = (IconTile)e.Data.GetData(typeof(IconTile));
            if (sender is Border border && border.Child is Frame frame)
            {
                switch(tile.DataContext.ToString())
                {
                    case "HOME":
                        {
                            frame.Navigate(new HomeShort());
                            break;
                        }
                    case "OSA":
                        {
                            frame.Navigate(new HomeShort());
                            break;
                        }
                    case "TC":
                        {
                            frame.Navigate(new HomeShort());
                            break;
                        }
                    case "TEMP":
                        {
                            frame.Navigate(new HomeShort());
                            break;
                        }
                    case "LASER":
                        {
                            frame.Navigate(new HomeShort());
                            break;
                        }
                    case "SWITCH":
                        {
                            frame.Navigate(new HomeShort());
                            break;
                        }
                    case "INTERR":
                        {
                            frame.Navigate(new HomeShort());
                            break;
                        }
                    case "POWER":
                        {
                            frame.Navigate(new HomeShort());
                            break;
                        }
                    default:
                        break;
                }
                var bordr = (Border)sender;
                switch(bordr.Name)
                {
                    case "Border0":
                        {
                            frames[0] = tile.DataContext.ToString();
                            break;
                        }
                    case "Border1":
                        {
                            frames[1] = tile.DataContext.ToString();
                            break;
                        }
                    case "Border2":
                        {
                            frames[2] = tile.DataContext.ToString();
                            break;
                        }
                    case "Border3":
                        {
                            frames[3] = tile.DataContext.ToString();
                            break;
                        }
                }
                tile.FillColor = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ff2a84f1"));
            }
        }

        private void IconTilesListBox_Selected(object sender, RoutedEventArgs e)
        {
            var tile = (IconTile)IconTilesListBox.SelectedItem;
            if (frames.Contains(tile.DataContext.ToString()))
            {
                tile.FillColor = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ff7b8792"));
                for (int i = 0; i < 4; i++)
                {
                    if (frames[i] == tile.DataContext.ToString())
                    {
                        frames[i] = string.Empty;
                        switch (i)
                        {
                            case 0:
                                {
                                    var frame = (Frame)Border0.Child;
                                    frame.Content = null;
                                    break;
                                }
                            case 1:
                                {
                                    var frame = (Frame)Border1.Child;
                                    frame.Content = null;
                                    break;
                                }
                            case 2:
                                {
                                    var frame = (Frame)Border2.Child;
                                    frame.Content = null;
                                    break;
                                }
                            case 3:
                                {
                                    var frame = (Frame)Border3.Child;
                                    frame.Content = null;
                                    break;
                                }
                        }
                        break;
                    }
                }
            }
        }
    }
}
