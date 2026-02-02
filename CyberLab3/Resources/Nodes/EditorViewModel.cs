using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CyberLab3.Resources.Nodes
{
    public class EditorViewModel
    {
        public PendingConnectionViewModel PendingConnection { get; }
        public ICommand DisconnectConnectorCommand { get; }
        public ObservableCollection<BaseNodeViewModel> Nodes { get; } = new ObservableCollection<BaseNodeViewModel>();
        public ObservableCollection<ConnectionViewModel> Connections { get; } = new ObservableCollection<ConnectionViewModel>();

        public EditorViewModel()
        {
            PendingConnection = new PendingConnectionViewModel(this);

            DisconnectConnectorCommand = new DelegateCommand<ConnectorViewModel>(connector =>
            {
                var connection = Connections.First(x => x.Source == connector || x.Target == connector);
                connection.Source.IsConnected = false;  // This is not correct if there are multiple connections to the same connector
                connection.Target.IsConnected = false;
                Connections.Remove(connection);
            });

            var welcome = new BaseNodeViewModel
            {
                Title = "Welcome",
                Location = new System.Windows.Point(100,100),
                Input = new ObservableCollection<ConnectorViewModel>
            {
                new ConnectorViewModel
                {
                    Type = ConnectorType.Input,
                }
            },
                Output = new ObservableCollection<ConnectorViewModel>
            {
                new ConnectorViewModel
                {
                    Type = ConnectorType.Output,
                }
            }
            };

            var nodify = new BaseNodeViewModel
            {
                Title = "To Nodify",
                Location = new System.Windows.Point(100, 200),
                Input = new ObservableCollection<ConnectorViewModel>
            {
                new ConnectorViewModel
                {
                    Type = ConnectorType.Input,
                }
            }
            };

            Nodes.Add(welcome);
            Nodes.Add(nodify);
        }

        public void Connect(ConnectorViewModel source, ConnectorViewModel target)
        {
            Connections.Add(new ConnectionViewModel(source, target));
        }
    }
}
