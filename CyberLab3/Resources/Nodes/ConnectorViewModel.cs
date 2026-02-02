using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CyberLab3.Resources.Nodes
{
    public class ConnectorViewModel : INotifyPropertyChanged
    {
        public ConnectorType Type { get; set; }

        private Point _anchor;
        public Point Anchor
        {
            set
            {
                _anchor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Anchor)));
            }
            get => _anchor;
        }

        private bool _isConnected;
        public bool IsConnected
        {
            set
            {
                _isConnected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
            }
            get => _isConnected;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public enum ConnectorType
    {
        Input,
        Output
    }
}
