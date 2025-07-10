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
    public class MeasurementTimer : Control
    {
        static MeasurementTimer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MeasurementTimer), new FrameworkPropertyMetadata(typeof(MeasurementTimer)));
        }

        public ICommand StartCommand
        {
            get => (ICommand)GetValue(StartCommandProperty);
            set => SetValue(StartCommandProperty, value);
        }

        public static readonly DependencyProperty StartCommandProperty = DependencyProperty.Register(nameof(StartCommand), typeof(ICommand), typeof(MeasurementTimer));

        public ICommand PauseCommand
        {
            get => (ICommand)GetValue(PauseCommandProperty);
            set => SetValue(PauseCommandProperty, value);
        }

        public static readonly DependencyProperty PauseCommandProperty = DependencyProperty.Register(nameof(PauseCommand), typeof(ICommand), typeof(MeasurementTimer));

        public ICommand RestartCommand
        {
            get => (ICommand)GetValue(RestartCommandProperty);
            set => SetValue(RestartCommandProperty, value);
        }

        public static readonly DependencyProperty RestartCommandProperty = DependencyProperty.Register(nameof(RestartCommand), typeof(ICommand), typeof(MeasurementTimer));

        public TimeSpan Time
        {
            get { return (TimeSpan)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(nameof(Time), typeof(TimeSpan), typeof(MeasurementTimer), new PropertyMetadata(TimeSpan.Zero, OnTimeChanged));

        private static void OnTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MeasurementTimer)d;
            int totalHours = (int)control.Time.TotalHours;
            control.TimeString = $"{totalHours:D2}:{control.Time.Minutes:D2}:{control.Time.Seconds:D2}";
            //control.TimeString = control.Time.ToString(@"hh\:mm\:ss"); // format dowolny
        }

        public string TimeString
        {
            get { return (string)GetValue(TimeStringProperty); }
            private set { SetValue(TimeStringPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey TimeStringPropertyKey = DependencyProperty.RegisterReadOnly(nameof(TimeString), typeof(string), typeof(MeasurementTimer), new PropertyMetadata("00:00:00"));

        public static readonly DependencyProperty TimeStringProperty = TimeStringPropertyKey.DependencyProperty;

        public bool IsRunning
        {
            get { return (bool)GetValue(IsRunningProperty); }
            set { SetValue(IsRunningProperty, value); }
        }

        public static readonly DependencyProperty IsRunningProperty = DependencyProperty.Register(nameof(IsRunning), typeof(bool), typeof(MeasurementTimer), new PropertyMetadata(false));
    }
}
