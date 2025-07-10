using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;

public class TimerViewModel : INotifyPropertyChanged
{
    private readonly DispatcherTimer _timer;
    private TimeSpan _time;

    public event PropertyChangedEventHandler PropertyChanged;

    public TimerViewModel()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (s, e) =>
        {
            Time = Time.Add(TimeSpan.FromSeconds(1));
        };
        StartTimerCommand = new RelayCommand(_ => Start());
        PauseTimerCommand = new RelayCommand(_ => Stop());
        RestartTimerCommand = new RelayCommand(_ => Reset());
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

    public ICommand StartTimerCommand { get; }
    public ICommand PauseTimerCommand { get; }
    public ICommand RestartTimerCommand { get; }
    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();
    public void Reset()
    {
        _timer.Stop();
        Time = TimeSpan.Zero;
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