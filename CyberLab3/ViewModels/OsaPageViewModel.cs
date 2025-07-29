using ScottPlot.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

public class OsaPageViewModel : INotifyPropertyChanged
{
    private string _IpAddress;
    private bool _isValidIp;

    public event PropertyChangedEventHandler PropertyChanged;
    public OsaPageViewModel()
    {

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