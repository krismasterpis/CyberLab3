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

namespace CyberLab3.Resources.Controls
{
    public class IconTile : Control
    {
        static IconTile()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconTile), new FrameworkPropertyMetadata(typeof(IconTile)));
        }

        public Geometry Icon
        {
            get { return (Geometry)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(Geometry), typeof(IconTile), new PropertyMetadata(null));




        public int IconWidth
        {
            get { return (int)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }

        public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register("IconWidth", typeof(int), typeof(IconTile), new PropertyMetadata(32));



        public int IconHeight
        {
            get { return (int)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }

        public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register("IconHeight", typeof(int), typeof(IconTile), new PropertyMetadata(32));



        public Brush FillColor
        {
            get { return (Brush)GetValue(FillColorProperty); }
            set { SetValue(FillColorProperty, value); }
        }

        public static readonly DependencyProperty FillColorProperty = DependencyProperty.Register("FillColor", typeof(Brush), typeof(IconTile), new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7b8792"))));


    }
}
