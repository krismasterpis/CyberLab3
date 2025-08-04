using Ivi.Visa;
using Microsoft.Extensions.Primitives;
using NationalInstruments.Visa;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Quic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace CyberLab3.Resources.Libraries
{
    public class OSA : IDisposable
    {
        const int BUFFER_LEN = 1024;
        const int OBE_BYTE = 1;
        const char BLOCK_CHAR = '#';

        private MessageBasedSession mbSession;

        public bool IsConnected { get; private set; }
        int timeout = 500;
        string ip;
        string ip2;
        public string identity;

        Dictionary<string, Trace> tracesLocal = new Dictionary<string, Trace>();

        public const string MODEL_NAME = "MS9740A";

        public const string MANUFACTURER = "ANRITSU CORPORATION";

        #region Enums for Settings

        public enum ResolutionSetting
        {
            Res_0_03_nm,
            Res_0_05_nm,
            Res_0_07_nm,
            Res_0_10_nm,
            Res_0_20_nm,
            Res_0_50_nm,
            Res_1_00_nm
        }

        public enum VbwSetting
        {
            VBW_10_Hz = 10,
            VBW_100_Hz = 100,
            VBW_200_Hz = 200,
            VBW_1_kHz = 1000,
            VBW_2_kHz = 2000,
            VBW_10_kHz = 10000,
            VBW_100_kHz = 100000,
            VBW_1_MHz = 1000000
        }

        public enum WavelengthDisplayUnit
        {
            InAir,
            InVacuum
        }

        public enum StorageMode
        {
            Off,
            SweepAverage,
            MaxHold,
            MinHold,
            Overlap
        }

        public enum WdmDetectionType
        {
            Peak,
            Threshold
        }

        #endregion

        #region Wavelength Parameters

        public const double WAVELENGTH_RANGE_MIN_nm = 600;

        public const double WAVELENGTH_RANGE_MAX_nm = 1750;

        public double SPAN_WAVELENGTH_nm;

        public double CENTER_WAVELENGTH_nm;

        public double START_WAVELENGTH_nm;

        public double STOP_WAVELENGTH_nm;

        public const double WAVELENGTH_ACCURACY_WITH_CAL_OPTION_pm = 20.0;

        public const double WAVELENGTH_STABILITY_pm = 5.0;

        public ResolutionSetting RESOLUTION_SETTING;

        #endregion

        #region Level Parameters

        public double REFERENCE_LEVEL_dBm;

        public double LOG_SCALE_PER_DIVISION_dB;

        public const double LEVEL_ACCURACY_dB = 0.4;

        public const double LEVEL_STABILITY_dB = 0.02;

        public const double LEVEL_LINEARITY_dB = 0.05;

        public const double MAX_OPTICAL_INPUT_LEVEL_dBm = 23;

        public const double RECEIVER_SENSITIVITY_dBm = -90;

        public bool INTERNAL_OPTICAL_ATTENUATOR_ENABLED;

        #endregion

        #region Sweep and Data Capture Parameters

        public const double SWEEP_SPEED_s_per_500nm = 0.3;

        public int SAMPLING_POINTS_COUNT;

        public int POINT_AVERAGE_COUNT;

        public int SWEEP_AVERAGE_COUNT;

        public int SMOOTHING_ENABLED;

        public VbwSetting VBW_SETTING;

        public StorageMode TRACE_STORAGE_MODE;

        #endregion

        #region Dynamic Range Parameters

        public const int DYNAMIC_RANGE_HIGH_MODE_at_1nm_dB = 70;

        public const int DYNAMIC_RANGE_HIGH_MODE_at_0_4nm_dB = 60;

        public const int DYNAMIC_RANGE_HIGH_MODE_at_0_2nm_dB = 42;

        public const int DYNAMIC_RANGE_NORMAL_MODE_at_1nm_dB = 62;

        public const int DYNAMIC_RANGE_NORMAL_MODE_at_0_4nm_dB = 58;

        #endregion

        #region Trigger Parameters

        public bool EXTERNAL_TRIGGER_ENABLED;

        public double EXTERNAL_TRIGGER_DELAY_s;

        public int MEASUREMENT_INTERVAL_TIME_s;

        #endregion

        /// <summary>
        /// Inicjalizuje nową instancję klasy InstrumentController.
        /// </summary>
        public OSA()
        {
            IsConnected = false;
            mbSession = null;
            for(int i = 0; i < 10; i++)
            {
                tracesLocal.Add($"Trace({(char)('A' + i)})", new Trace($"Trace({(char)('A' + i)})"));
            }
        }

        /// <summary>
        /// Nawiązuje połączenie z instrumentem używając podanego adresu zasobu VISA.
        /// </summary>
        /// <param name="visaResourceName">Pełny adres zasobu VISA, np. "TCPIP0::172.16.2.54::inst0::INSTR".</param>
        /// <returns>True, jeśli połączenie się powiodło; w przeciwnym razie false.</returns>
        public static bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return pingable;
        }
        public void Connect(string _ip, int _timeout = -1)
        {
            try
            {
                if (String.IsNullOrEmpty(_ip))
                {
                    throw new Exception("Adres urządzenia nie może być pusty!");
                }
                ip = _ip;
                ip2 = ip.Split("::")[1];
                if (_ip.Contains("::SOCKET"))
                {
                    throw new Exception("Adres urzadzenia nie może odnosić się do połaczenia typu Socket!");
                }
                else if (!_ip.Contains("::INSTR"))
                {
                    throw new Exception("Adres urządzenia musi odnosic się do połaczenia typu INSTR!");
                }
                if (PingHost(ip2))
                {
                    ResourceManager rManager = new ResourceManager();
                    mbSession = (MessageBasedSession)rManager.Open(ip);
                    if(_timeout != -1)
                    {
                        mbSession.TimeoutMilliseconds = _timeout;
                    }
                    switch (mbSession.HardwareInterfaceType)
                    {
                        case HardwareInterfaceType.Tcp:
                            mbSession.TerminationCharacterEnabled = true;
                            break;
                        default:
                            break;
                    }
                    IsConnected = true;
                }
                ReadParameters();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Wykryto niespodziewany błąd : {0} podczas łączenia z {1}", e.ToString(), ip);
            }
        }

        /// <summary>
        /// Zamyka połączenie z instrumentem i zwalnia zasoby.
        /// </summary>
        public async void Disconnect()
        {
            if (!IsConnected || mbSession == null)
            {
                return;
            }

            try
            {
                // Prawidłowym sposobem zamknięcia sesji jest użycie metody Dispose()
                Task.Run(() => mbSession.Dispose());
                Console.WriteLine("Rozłączono.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas rozłączania: {ex.Message}");
            }
            finally
            {
                IsConnected = false;
                mbSession = null;
            }
        }

        public string Query(string qCommand)
        {
            string response = null;
            try
            {
                mbSession.FormattedIO.WriteLine(qCommand);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Wykryto niespodziewany błąd : {e} podczas kolejkowania {qCommand}");
            }
            try
            {
                response = Read();
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Wykryto niespodziewany błąd : {e} podczas kolejkowania {qCommand}");
            }
            return string.Empty;

        }

        public void SetTimeout(int _timeout)
        {
            timeout = _timeout;
            mbSession.TimeoutMilliseconds = timeout;
        }
        
        string Read()
        {
            string data = null;
            try
            {
                // Read first character, determine if it's "#", preceding block data
                data += mbSession.FormattedIO.ReadChar();

                // If not, read the rest of the line
                if (data != BLOCK_CHAR.ToString())
                    data += mbSession.FormattedIO.ReadLine();
                // Else...
                else
                {
                    string sizeBuffer1 = "";
                    string sizeBuffer2 = "";
                    // Read data block size descriptor
                    sizeBuffer1 = Convert.ToString(mbSession.FormattedIO.ReadChar());
                    data += sizeBuffer1;

                    // Read data block size
                    for (int i = 0; i < Convert.ToInt32(sizeBuffer1); i++)
                    {
                        sizeBuffer2 += mbSession.FormattedIO.ReadChar();
                    }
                    data += sizeBuffer2;

                    // Read data block according to the size determined + 1 final character (newline)
                    for (int i = 0; i <= Convert.ToInt32(sizeBuffer2); i++)
                    {
                        data += mbSession.FormattedIO.ReadChar();
                    }

                    // Optional Buffer Discard for no interference in future query reads
                    mbSession.FormattedIO.DiscardBuffers(); // to investigate further
                }
                data = data.TrimEnd('\n');
                data = data.TrimEnd('\r');
            }
            catch (Exception e)
            {
                Console.WriteLine($"Wykryto niespodziewany błąd : {e} podczas odczytu");
            }
            return data;
        }

        public void Write(string wCommand)
        {
            try
            {
                mbSession.FormattedIO.WriteLine(wCommand);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Wykryto niespodziewany błąd : {e} podczas pisania {1}", wCommand);
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        public void ReadParameters()
        {
            string temp;
            double resultD;
            int resultI;
            temp = Query(AnritsuMs9740AScpiCommands.SPAN_WAVELENGTH_QUERY);
            if (double.TryParse(temp, CultureInfo.InvariantCulture, out resultD)) SPAN_WAVELENGTH_nm = resultD;
            temp = Query(AnritsuMs9740AScpiCommands.CENTER_WAVELENGTH_QUERY);
            if (double.TryParse(temp, CultureInfo.InvariantCulture, out resultD)) CENTER_WAVELENGTH_nm = resultD;
            temp = Query(AnritsuMs9740AScpiCommands.START_WAVELENGTH_QUERY);
            if (double.TryParse(temp, CultureInfo.InvariantCulture, out resultD)) START_WAVELENGTH_nm = resultD;
            temp = Query(AnritsuMs9740AScpiCommands.STOP_WAVELENGTH_QUERY);
            if (double.TryParse(temp, CultureInfo.InvariantCulture, out resultD)) STOP_WAVELENGTH_nm = resultD;
            temp = Query(AnritsuMs9740AScpiCommands.RESOLUTION_QUERY);
            switch (temp)
            {
                case "1.0":
                    RESOLUTION_SETTING = ResolutionSetting.Res_1_00_nm;
                    break;
                case "0.50":
                    RESOLUTION_SETTING = ResolutionSetting.Res_0_50_nm;
                    break;
                case "0.20":
                    RESOLUTION_SETTING = ResolutionSetting.Res_0_20_nm;
                    break;
                case "0.10":
                    RESOLUTION_SETTING = ResolutionSetting.Res_0_10_nm;
                    break;
                case "0.07":
                    RESOLUTION_SETTING = ResolutionSetting.Res_0_07_nm;
                    break;
                case "0.05":
                    RESOLUTION_SETTING = ResolutionSetting.Res_0_05_nm;
                    break;
                case "0.03":
                    RESOLUTION_SETTING = ResolutionSetting.Res_0_03_nm;
                    break;
                default:
                    break;
            }    
            temp = Query(AnritsuMs9740AScpiCommands.SAMPLING_POINTS_QUERY);
            if (int.TryParse(temp, out resultI)) SAMPLING_POINTS_COUNT = resultI;
            temp = Query(AnritsuMs9740AScpiCommands.POINT_AVERAGE_QUERY);
            if (int.TryParse(temp, out resultI)) POINT_AVERAGE_COUNT = resultI;
            temp = Query(AnritsuMs9740AScpiCommands.SWEEP_AVERAGE_QUERY);
            if (int.TryParse(temp, out resultI)) SWEEP_AVERAGE_COUNT = resultI;
            temp = Query(AnritsuMs9740AScpiCommands.SMOOTHING_QUERY);
            if (temp != "OFF")
            {
                if (int.TryParse(temp, out resultI)) SMOOTHING_ENABLED = resultI;
            }
            else
            {
                SMOOTHING_ENABLED = -1;
            }    
            temp = Query(AnritsuMs9740AScpiCommands.VIDEO_BANDWIDTH_QUERY);
            switch (temp)
            {
                case "10HZ":
                    VBW_SETTING = VbwSetting.VBW_10_Hz;
                    break;
                case "100HZ":
                    VBW_SETTING = VbwSetting.VBW_100_Hz; break;
                case "200HZ":
                    VBW_SETTING = VbwSetting.VBW_200_Hz; break;
                case "1KHZ":
                    VBW_SETTING = VbwSetting.VBW_1_kHz; break;
                case "2KHZ":
                    VBW_SETTING = VbwSetting.VBW_2_kHz; break;
                case "10KHZ":
                    VBW_SETTING = VbwSetting.VBW_10_kHz; break;
                case "100KHZ":
                    VBW_SETTING = VbwSetting.VBW_100_kHz; break;
                case "1MHZ":
                    VBW_SETTING = VbwSetting.VBW_1_MHz; break;
                default:
                    break;
            }
        }
        private List<double> generateXS(double _center, double _span, int _points)
        {
            double min = Convert.ToDouble(Query("STA?").Replace('.', ','));
            double max = Convert.ToDouble(Query("STO?").Replace('.', ','));
            double step = (max - min) / (_points);
            int j = 0;
            List<double> xs = new List<double>();
            for (double i = min; i <= max; i += step)
            {
                if(j<_points)
                {
                    if (j == _points - 1)
                    {
                        xs.Add(Math.Round(max, 5));
                    }
                    else if (j == 0)
                    {
                        xs.Add(Math.Round(min, 5));
                    }
                    else
                    {
                        xs.Add(Math.Round(i, 5));
                    }
                }
                j++;
            }
            return xs;
        }
        public void ReadSingle(Measurement measurement, Dictionary<string, TraceType> types)
        {
            bool ready = false;
            try
            {
                Write(AnritsuMs9740AScpiCommands.SINGLE_SWEEP);
                Write("*CLS");
                ready = WaitForEndOfOperation();
                string result;
                List<double> Xs = new List<double>();
                Xs = generateXS(this.CENTER_WAVELENGTH_nm, this.SPAN_WAVELENGTH_nm, this.SAMPLING_POINTS_COUNT);
                foreach(var trace in measurement.traces.Values)
                {
                    result = "";
                    if(trace.name != null)
                    {
                        if (types[trace.name] == TraceType.Write)
                        {
                            switch (trace.name)
                            {
                                case "Trace(A)":
                                    result = Query(AnritsuMs9740AScpiCommands.MEMORY_DATA_A_CSV_QUERY);
                                    break;
                                case "Trace(B)":
                                    result = Query(AnritsuMs9740AScpiCommands.MEMORY_DATA_B_CSV_QUERY);
                                    break;
                                case "Trace(C)":
                                    result = Query(AnritsuMs9740AScpiCommands.MEMORY_DATA_C_CSV_QUERY);
                                    break;
                                case "Trace(D)":
                                    result = Query(AnritsuMs9740AScpiCommands.MEMORY_DATA_D_CSV_QUERY);
                                    break;
                                case "Trace(E)":
                                    result = Query(AnritsuMs9740AScpiCommands.MEMORY_DATA_E_CSV_QUERY);
                                    break;
                                case "Trace(F)":
                                    result = Query(AnritsuMs9740AScpiCommands.MEMORY_DATA_F_CSV_QUERY);
                                    break;
                                case "Trace(G)":
                                    result = Query(AnritsuMs9740AScpiCommands.MEMORY_DATA_G_CSV_QUERY);
                                    break;
                                case "Trace(H)":
                                    result = Query(AnritsuMs9740AScpiCommands.MEMORY_DATA_H_CSV_QUERY);
                                    break;
                                case "Trace(I)":
                                    result = Query(AnritsuMs9740AScpiCommands.MEMORY_DATA_I_CSV_QUERY);
                                    break;
                                case "Trace(J)":
                                    result = Query(AnritsuMs9740AScpiCommands.MEMORY_DATA_J_CSV_QUERY);
                                    break;
                                case null:
                                    break;
                            }
                            string[] temp = result.Split(',');
                            List<double> Ys = new List<double>();
                            for (int i = 0; i < temp.Length-1; i++)
                            {
                                double.TryParse(temp[i], CultureInfo.InvariantCulture, out var value);
                                Ys.Add(value);
                            }
                            trace.SetData(Xs, Ys);
                        }
                        else if (trace.type == TraceType.Blank)
                        {
                            trace.Clear();
                        }
                        else if (trace.type == TraceType.Calculate)
                        {
                            var char1 = trace.calculateTrace1;
                            var char2 = trace.calculateTrace2;
                            var sign = trace.calculateSign;
                            List<double> x = new List<double>();
                            List<double> y = new List<double>();
                            for (int i = 0; i < measurement.traces[$"Trace({char1})"].wavelengths.Count; i++)
                            {
                                x.Add(measurement.traces[$"Trace({char1})"].wavelengths[i]);
                                y.Add(measurement.traces[$"Trace({char1})"].attenuationsRaw[i] - measurement.traces[$"Trace({char2})"].attenuationsRaw[i]);
                            }
                            trace.SetData(x, y);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Wykryto niespodziewany błąd : {e} podczas odczytywanie SINGLE {1}");
            }
        }

        public void SendTraceType(char traceChar, TraceType type)
        {
            switch(type)
            {
                case TraceType.Blank:
                    Write($"TTP {traceChar},BLANK");
                    break;
                case TraceType.Write:
                    Write($"TTP {traceChar},WRITE");
                    break;
                case TraceType.Fix:
                    Write($"TTP {traceChar},BLANK");
                    break;
                case TraceType.Calculate:
                    Write($"TTP {traceChar},BLANK");
                    break;
            }
        }
        public bool WaitForEndOfOperation()
        {
            /*            string status = "";
                        while(status != "1")
                        {
                            status = Query("ESR2?");
                            Debug.WriteLine(status);
                            Task.Delay(10);
                        }
            */
            while (true)
            {
                if (Query("ESR2?") != "0")
                {
                    break;
                }
            }
            return true;
        }
    }
}
