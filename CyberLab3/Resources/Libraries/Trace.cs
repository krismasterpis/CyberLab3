using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberLab3.Resources.Libraries
{
    public class Trace
    {
        public List<double> wavelengths;
        public List<double> attenuationsRaw;
        public List<double> attenuationsSmooth;
        public string name;
        public TraceType type = TraceType.Blank;
        public char calculateTrace1;
        public char calculateTrace2;
        public char calculateSign;
        public Trace(string _name)
        {
            name = _name;
            wavelengths = new List<double>();
            attenuationsRaw = new List<double>();
            attenuationsSmooth = new List<double>();
        }

        public void SetData(List<double> _wavelengths, List<double> _attenuationRaw)
        {
            wavelengths = new List<double>(_wavelengths);
            attenuationsRaw = new List<double>(_attenuationRaw);
            attenuationsSmooth = new List<double>();
        }

        public void Clear()
        {
            wavelengths.Clear();
            attenuationsRaw.Clear();
            attenuationsSmooth.Clear();
        }
    }

    public enum TraceType
    {
        Blank,
        Write,
        Fix,
        Calculate
    }
}
