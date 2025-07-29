using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

public class TimerViewModel : INotifyPropertyChanged
{
    private readonly DispatcherTimer _timer;
    private readonly DispatcherTimer _counter;
    private TimeSpan _time;
    private TimeSpan _estTime;
    private string _estTimeStr;
    private Visibility _isCounterEnabled;

    public event PropertyChangedEventHandler PropertyChanged;

    public TimerViewModel()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _counter = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (s, e) =>
        {
            Time = Time.Add(TimeSpan.FromSeconds(1));
        };
        _counter.Tick += (s, e) =>
        {
            estTime = estTime.Subtract(TimeSpan.FromSeconds(1));
            estTimeStr = estTime.ToString();
        };
        StartTimerCommand = new RelayCommand(_ => Start());
        PauseTimerCommand = new RelayCommand(_ => Stop());
        RestartTimerCommand = new RelayCommand(_ => Reset());
        _isCounterEnabled = Visibility.Hidden;
    }

    public TimeSpan Time
    {
        get => _time;
        set
        {
            if (_time != value)
            {
                _time = value;
                OnPropertyChanged();
            }
        }
    }

    public TimeSpan estTime
    {
        get => _estTime;
        set
        {
            if (_estTime != value)
            {
                _estTime = value;
                estTimeStr = value.ToString();
                OnPropertyChanged();
            }
        }
    }

    public string estTimeStr
    {
        get => _estTimeStr;
        set
        {
            if (_estTimeStr != "Est. " + value)
            {
                _estTimeStr = "Est. " + value;
                OnPropertyChanged();
            }
        }
    }

    public Visibility isCounterEnabled
    {
        get => _isCounterEnabled;
        set
        {
            if (_isCounterEnabled != value)
            {
                _isCounterEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand StartTimerCommand { get; }
    public ICommand PauseTimerCommand { get; }
    public ICommand RestartTimerCommand { get; }
    public void Start()
    {
        _timer.Start();
        if (estTime > TimeSpan.Zero)
        {
            _counter.Start();
            isCounterEnabled = Visibility.Visible;
        }
    }
    public void Stop()
    {
        _timer.Stop();
        _counter.Stop();
    }
public void Reset()
    {
        _timer.Stop();
        _counter.Stop();
        Time = TimeSpan.Zero;
        if(isCounterEnabled == Visibility.Visible)
        {
            estTime = TimeSpan.Zero;
            isCounterEnabled = Visibility.Hidden;
        }
    }

    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object parameter) => _execute(parameter);

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}