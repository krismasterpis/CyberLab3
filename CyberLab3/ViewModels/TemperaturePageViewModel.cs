using ScottPlot.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TemperaturePageViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;


    public WpfPlot TempPlot { get; set; } = new WpfPlot();
    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
