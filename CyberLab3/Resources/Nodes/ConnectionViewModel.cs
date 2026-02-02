using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberLab3.Resources.Nodes
{
    public class ConnectionViewModel
    {
        public ConnectionViewModel(ConnectorViewModel source, ConnectorViewModel target)
        {
            Source = source;
            Target = target;

            Source.IsConnected = true;
            Target.IsConnected = true;
        }
        public ConnectorViewModel Source { get; set; }
        public ConnectorViewModel Target { get; set; }
    }
}
