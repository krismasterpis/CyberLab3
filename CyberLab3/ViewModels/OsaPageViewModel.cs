using CyberLab3.Resources.Libraries;
using ScottPlot.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

public class OsaPageViewModel : INotifyPropertyChanged
{
    private LocalTimer _localTimer;
    private TimeSpan _localEstTime;
    private TimeSpan _localTime;
    private Visibility _isLocalTimerEnabled;
    private string _IpAddress;
    private bool _isValidIp;
    private int _localMeasNum;
    private long _elapsedMs;
    private string _limitStr = "0";

    public event PropertyChangedEventHandler PropertyChanged;
    public OsaPageViewModel()
    {

    }
    public string LimitStr
    {
        get => _limitStr;
        set
        {
            if (_limitStr != value)
            {
                _limitStr = value;
                OnPropertyChanged();
            }
        }
    }
    public TimeSpan LocalTime
    {
        get
        {
            if(_localTimer != null)
            {
                return _localTimer.Time;
            }
            return TimeSpan.Zero;
        }
    }
    public int LocalMeasNum
    {
        get => _localMeasNum;
        set
        {
            if (_localMeasNum != value)
            {
                _localMeasNum = value;
                OnPropertyChanged();
            }
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
            if(_localTimer != value)
            {
                _localTimer = value;
                OnPropertyChanged();
            }
        }
    }

    public string IpAddress
    {
        get 
        { 
            return _IpAddress; 
        }
        set 
        { 
            _IpAddress = value;
            OnPropertyChanged();
            IsValidIp = IPAddress.TryParse(value, out _);
        }
    }
    public bool IsValidIp
    {
        get => _isValidIp;
        set
        {
            _isValidIp = value;
            OnPropertyChanged(nameof(IsValidIp));
        }
    }

    public WpfPlot OSAplot { get; set; } = new WpfPlot();

    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
    => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
//<ScottPlot:WpfPlot x:Name="OSAplot" Margin="10"/>