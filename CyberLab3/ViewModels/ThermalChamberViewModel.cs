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
    public int tauHeating;
    public int tauCooling;
    private LocalTimer _localTimer;
    private TimeSpan _localEstTime;
    private TimeSpan _localTime;
    private LocalTimer _localTimer2;
    private TimeSpan _localEstTime2;
    private TimeSpan _localTime2;
    private LocalTimer _localTimer3;
    private TimeSpan _localEstTime3;
    private TimeSpan _localTime3;
    private long _elapsedMs;
    private double _elapsedMsAvg;
    private long _measurementFails;
    private Visibility _isLocalTimerEnabled;
    private bool _isConnected = false;
    private bool _isIntervalSetted = false;
    private bool _isSetpointsSetted = false;
    private string _thermalChamberStatus;
    private float _currTemperature;
    private float _currSetPoint;
    private float _nextSetPoint;
    private int _interval = 1;
    private bool _loopMode = false;
    private bool _reverseMode = false;
    private bool _isAutosaveEnabled = false;
    public int Interval
    {
        get => _interval;
        set
        {
            if (_interval != value)
            {
                _interval = value;
                OnPropertyChanged();
            }
        }
    }
    public float CurrTemperature
    {
        get => _currTemperature;
        set
        {
            if (_currTemperature != value)
            {
                _currTemperature = value;
                OnPropertyChanged();
            }
        }
    }
    public float NextSetPoint
    {
        get => _nextSetPoint;
        set
        {
            if (_nextSetPoint != value)
            {
                _nextSetPoint = value;
                OnPropertyChanged();
            }
        }
    }
    public float CurrSetPoint
    {
        get => _currSetPoint;
        set
        {
            if (_currSetPoint != value)
            {
                _currSetPoint = value;
                OnPropertyChanged();
            }
        }
    }
    public string ThermalChamberStatus
    {
        get => _thermalChamberStatus;
        set
        {
            if (_thermalChamberStatus != value)
            {
                _thermalChamberStatus = value;
                OnPropertyChanged();
            }
        }
    }
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
    public bool LoopMode
    {
        get => _loopMode;
        set
        {
            if (_loopMode != value)
            {
                _loopMode = value;
                OnPropertyChanged();
            }
        }
    }
    public bool ReverseMode
    {
        get => _reverseMode;
        set
        {
            if (_reverseMode != value)
            {
                _reverseMode = value;
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
    public bool IsSetpointsSetted
    {
        get => _isSetpointsSetted;
        set
        {
            if (_isSetpointsSetted != value)
            {
                _isSetpointsSetted = value;
                OnPropertyChanged();
            }
        }
    }
    public bool IsAutosaveEnabled
    {
        get => _isAutosaveEnabled;
        set
        {
            if (_isAutosaveEnabled != value)
            {
                _isAutosaveEnabled = value;
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
    public LocalTimer LocalTimer2
    {
        get => _localTimer2;
        set
        {
            if (_localTimer2 != value)
            {
                _localTimer2 = value;
                OnPropertyChanged();
            }
        }
    }

    public LocalTimer LocalTimer3
    {
        get => _localTimer3;
        set
        {
            if (_localTimer3 != value)
            {
                _localTimer3 = value;
                OnPropertyChanged();
            }
        }
    }
    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
