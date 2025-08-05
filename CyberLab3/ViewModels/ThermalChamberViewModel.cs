using CyberLab3.Resources.Libraries;
using ScottPlot.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

public class ThermalChamberViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public WpfPlot MainPlot { get; set; } = new WpfPlot();
    public WpfPlot SetPointsPlot { get; set; } = new WpfPlot();
    private LocalTimer _localTimer;
    private TimeSpan _localEstTime;
    private TimeSpan _localTime;
    private long _elapsedMs;
    private double _elapsedMsAvg;
    private long _measurementFails;
    private Visibility _isLocalTimerEnabled;
    private bool _isConnected = false;
    private bool _isIntervalSetted = false;

    public TimeSpan LocalTime
    {
        get
        {
            if (_localTimer != null)
            {
                return _localTimer.Time;
            }
            return TimeSpan.Zero;
        }
    }
    public long ElapsedMs
    {
        get => _elapsedMs;
        set
        {
            if (_elapsedMs != value)
            {
                _elapsedMs = value;
                OnPropertyChanged();
            }
        }
    }
    public double ElapsedMsAvg
    {
        get => _elapsedMsAvg;
        set
        {
            if (_elapsedMsAvg != value)
            {
                _elapsedMsAvg = value;
                OnPropertyChanged();
            }
        }
    }
    public long MeasurementFails
    {
        get => _measurementFails;
        set
        {
            if (_measurementFails != value)
            {
                _measurementFails = value;
                OnPropertyChanged();
            }
        }
    }
    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (_isConnected != value)
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }
    }
    public bool IsIntervalSetted
    {
        get => _isIntervalSetted;
        set
        {
            if (_isIntervalSetted != value)
            {
                _isIntervalSetted = value;
                OnPropertyChanged();
            }
        }
    }
    public TimeSpan LocalEstTime
    {
        get => _localEstTime;
        set
        {
            if (_localEstTime != value)
            {
                _localEstTime = value;
                OnPropertyChanged();
            }
        }
    }
    public Visibility IsLocalTimerEnabled
    {
        get => _isLocalTimerEnabled;
        set
        {
            if (_isLocalTimerEnabled != value)
            {
                _isLocalTimerEnabled = value;
                OnPropertyChanged();
            }
        }
    }
    public LocalTimer LocalTimer
    {
        get => _localTimer;
        set
        {
            if (_localTimer != value)
            {
                _localTimer = value;
                OnPropertyChanged();
            }
        }
    }

    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
