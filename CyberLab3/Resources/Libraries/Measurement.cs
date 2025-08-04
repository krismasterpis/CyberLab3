using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberLab3.Resources.Libraries
{
    public class Measurement
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public double Temperature { get; set; }
        public Dictionary<string, Trace> traces { get; set; } = new();
        public Measurement()
        {
            for(int i = 0; i < 10; i++)
            {
                var name = $"Trace({(char)('A' + i)})";
                traces.Add(name, new Trace(name));
            }
        }
    }
}
