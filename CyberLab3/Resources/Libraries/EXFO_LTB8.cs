using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CyberLab3.Resources.Libraries
{
    public class EXFO_LTB8
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private readonly string _ipAddress;
        private readonly int _port;
        public Dictionary<int,string> InstrumentCatalog = new Dictionary<int, string>();

        public EXFO_LTB8(string ipAddress, int port = 5025)
        {
            if (!ValidateIPv4(ipAddress))
            {
                throw new ArgumentException("Invalid IPv4 address format.", nameof(ipAddress));
            }
            _ipAddress = ipAddress;
            _port = port;
        }

        /// <summary>
        /// Establishes a connection with the LTB-8 device.
        /// </summary>
        public void Connect()
        {
            try
            {
                _client = new TcpClient(_ipAddress, _port);
                _stream = _client.GetStream();
                // Set a timeout for reading (e.g., 5 seconds)
                _stream.ReadTimeout = 5000;
                InstrumentCatalog = GetModuleInventory();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to connect to LTB-8 at {_ipAddress}: {ex.Message}");
            }
        }

        private bool ValidateIPv4(string ipString)
        {
            if (String.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }

            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }

            byte tempForParsing;

            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }

        /// <summary>
        /// Sends a SCPI command to the device. 
        /// Most devices require a newline character (\n) at the end.
        /// </summary>
        public void SendCommand(string command)
        {
            if (_stream == null) throw new InvalidOperationException("Not connected.");

            byte[] data = Encoding.ASCII.GetBytes(command + "\n");
            _stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Sends a query and waits for the response string.
        /// </summary>
        public string Query(string query)
        {
            SendCommand(query);

            byte[] buffer = new byte[1024];
            int bytesRead = _stream.Read(buffer, 0, buffer.Length);

            return Encoding.ASCII.GetString(buffer, 0, bytesRead).TrimEnd('\n', '\r');
        }

        public string Identify()
        {
            return Query(EXFO_LTB8_consts.IdentificationQuery);
        }

        /// <summary>
        /// Sets the data format for transferring large blocks of data.
        /// </summary>
        /// <param name="format">Use "ASC" for ASCII or "PACK" for Packed.</param>
        public void SetDataFormat(string format)
        {
            // Example: :FORMat:DATA ASCii
            SendCommand($"{EXFO_LTB8_consts.FormatData} {format}");
        }

        /// <summary>
        /// Queries the current data format.
        /// </summary>
        public string GetDataFormat()
        {
            return Query(EXFO_LTB8_consts.FormatDataQuery);
        }

        /// <summary>
        /// Returns a list of all logical instrument names and group names available on the LTB-8.
        /// </summary>
        public List<string> GetInstrumentCatalog()
        {
            string response = Query(EXFO_LTB8_consts.InstrumentCatalog);
            // Response is usually a comma-separated string: "Slot1","Slot2","Module1"
            return response.Split(',').Select(x => x.Trim('\"')).ToList();
        }

        /// <summary>
        /// Returns a full list of logical instruments with their corresponding numbers (LINS).
        /// </summary>
        public List<string> GetInstrumentCatalogFull()
        {
            string response = Query(EXFO_LTB8_consts.InstrumentCatalogFull);
            return response.Split(',').Select(x => x.Trim('\"')).ToList();
        }

        /// <summary>
        /// Sets the internal system date.
        /// </summary>
        public void SetSystemDate(DateTime date)
        {
            // Format: :SYSTem:DATE <year>,<month>,<day>
            string command = $"{EXFO_LTB8_consts.SystemDate} {date.Year},{date.Month},{date.Day}";
            SendCommand(command);
        }

        /// <summary>
        /// Queries the internal system date and returns it as a DateTime object.
        /// </summary>
        public DateTime GetSystemDate()
        {
            string response = Query(EXFO_LTB8_consts.SystemDateQuery);
            // Expected response format: +2024,+12,+31
            string[] parts = response.Split(',');
            return new DateTime(
                int.Parse(parts[0]),
                int.Parse(parts[1]),
                int.Parse(parts[2])
            );
        }

        /// <summary>
        /// Sets the internal system time.
        /// </summary>
        public void SetSystemTime(int hour, int minute, int second)
        {
            // Format: :SYSTem:TIME <hour>,<minute>,<second>
            string command = $"{EXFO_LTB8_consts.SystemTime} {hour},{minute},{second}";
            SendCommand(command);
        }

        /// <summary>
        /// Queries the internal system time.
        /// </summary>
        public string GetSystemTime()
        {
            return Query(EXFO_LTB8_consts.SystemTimeQuery);
        }

        /// <summary>
        /// Queries the next error from the system error queue.
        /// Returns a string like: 0,"No error"
        /// </summary>
        public string GetNextError()
        {
            return Query(EXFO_LTB8_consts.SystemErrorNext);
        }

        /// <summary>
        /// Queries the SCPI version to which the instrument complies.
        /// </summary>
        public string GetScpiVersion()
        {
            return Query(EXFO_LTB8_consts.SystemVersionQuery);
        }
        /// <summary>
        /// Closes the connection and releases resources.
        /// </summary>
        

        /// /// <summary>
        /// Performs a full hardware and software reset. 
        /// Combines *RST (reset) and *CLS (clear errors).
        /// </summary>
        public void HardReset()
        {
            // We clear status first so old errors don't haunt us after reset
            SendCommand(EXFO_LTB8_consts.ClearStatus);
            SendCommand(EXFO_LTB8_consts.ResetDevice);

            // Wait until the device is ready to accept new commands
            WaitForReady();
        }

        /// <summary>
        /// Uses *OPC? to block execution until the device completes all pending actions.
        /// Essential after calling long operations like self-tests or module loading.
        /// </summary>
        public void WaitForReady(int timeoutMs = 10000)
        {
            int originalTimeout = _stream.ReadTimeout;
            _stream.ReadTimeout = timeoutMs; // Set temporary longer timeout for OPC

            try
            {
                string result = Query(EXFO_LTB8_consts.OperationCompleteQuery);
                if (result != "1")
                    throw new Exception("Device synchronization failed - OPC returned unexpected value.");
            }
            finally
            {
                _stream.ReadTimeout = originalTimeout; // Restore original timeout
            }
        }

        /// <summary>
        /// Comprehensive error checker. Polls the error queue until it's empty.
        /// Useful for post-configuration verification.
        /// </summary>
        public List<string> GetAllErrors()
        {
            List<string> errors = new List<string>();
            while (true)
            {
                // Query :SYSTem:ERRor:NEXT?
                string error = Query(EXFO_LTB8_consts.SystemErrorNext);

                // Standard SCPI "no error" response is: 0,"No error"
                if (error.StartsWith("0") || string.IsNullOrEmpty(error))
                    break;

                errors.Add(error);
            }
            return errors;
        }

        /// <summary>
        /// Checks the Status Byte (*STB?) to see if the Error Queue bit is set.
        /// Fast way to check if something went wrong without reading the whole queue.
        /// </summary>
        public bool HasErrors()
        {
            string stbResponse = Query(EXFO_LTB8_consts.GetStatusByte);
            if (int.TryParse(stbResponse, out int stb))
            {
                // Bit 2 (value 4) of Status Byte often indicates Error Queue
                return (stb & 4) != 0;
            }
            return false;
        }

        /// <summary>
        /// Performs a self-test and returns a boolean success value.
        /// Useful for automated "Go/No-Go" testing stations.
        /// </summary>
        /// 

        public Dictionary<int, string> GetModuleInventory()
        {
            // Pobieramy surowy ciąg, np: "Slot1","FTB-88200G","Slot2","FTB-8130NGE"
            string response = Query(EXFO_LTB8_consts.InstrumentCatalogFull);

            if (string.IsNullOrEmpty(response) || response.Contains("None"))
                return new Dictionary<int, string>();

            List<string> parts = response.Split(',')
                                         .Select(p => p.Trim('\"'))
                                         .ToList();

            // Logika: Klucze (1, 3, 5) to nazwy modułów, Wartości (0, 2, 4) to sloty
            var inventory = new Dictionary<int, string>();

            for (int i = 0; i < parts.Count - 1; i += 2)
            {
                int.TryParse(parts[i+1],out int slot);     // Indeks 0, 2, 4...
                string module = parts[i]; // Indeks 1, 3, 5...

                // Unikamy duplikatów kluczy, jeśli kilka slotów ma ten sam typ modułu
                inventory.Add(slot, module);
            }

            inventory = inventory.OrderBy(kv => kv.Key).ToDictionary();

            return inventory;
        }
        public bool RunDiagnosticTest()
        {
            string result = Query(EXFO_LTB8_consts.SelfTestQuery); // *TST?
                                                               // Typically returns "0" for pass, anything else for fail
            return result == "0";
        }
        public void Dispose()
        {
            _stream?.Close();
            _client?.Close();
        }
    }
    class EXFO_LTB8_consts
    {
        // --- IEEE 488.2 REQUIRED COMMANDS ---

        /// <summary>Clears all status registers and the error queue.</summary>
        public static string ClearStatus = "*CLS";

        /// <summary>Sets the bits in the Standard Event Status Enable register.</summary>
        public static string SetEventStatusEnable = "*ESE";

        /// <summary>Queries the Standard Event Status Enable register.</summary>
        public static string GetEventStatusEnable = "*ESE?";

        /// <summary>Queries and clears the Standard Event Status register.</summary>
        public static string GetEventStatusRegister = "*ESR?";

        /// <summary>Returns the instrument identification (Manufacturer, Model, SN, Firmware).</summary>
        public static string IdentificationQuery = "*IDN?";

        /// <summary>Sets the Operation Complete bit in the ESR when all pending operations finish.</summary>
        public static string OperationComplete = "*OPC";

        /// <summary>Returns '1' to the output queue when all pending operations are finished.</summary>
        public static string OperationCompleteQuery = "*OPC?";

        /// <summary>Resets the instrument to its default factory settings.</summary>
        public static string ResetDevice = "*RST";

        /// <summary>Sets the bits in the Service Request Enable register.</summary>
        public static string SetServiceRequestEnable = "*SRE";

        /// <summary>Queries the Service Request Enable register.</summary>
        public static string GetServiceRequestEnable = "*SRE?";

        /// <summary>Queries the Status Byte and the Master Summary Status bit.</summary>
        public static string GetStatusByte = "*STB?";

        /// <summary>Initiates an internal self-test and returns the result (0 means pass).</summary>
        public static string SelfTestQuery = "*TST?";

        /// <summary>Instructs the instrument to wait until all pending operations are completed.</summary>
        public static string WaitToContinue = "*WAI";


        // --- LTB-8 SPECIFIC COMMANDS (SCPI) ---

        /// <summary>Sets the data format for transferring large blocks of data (ASCii or PACKed).</summary>
        public static string FormatData = ":FORMat:DATA";

        /// <summary>Queries the current data format and length.</summary>
        public static string FormatDataQuery = ":FORMat:DATA?";

        /// <summary>Returns a list of all logical instrument names and group names.</summary>
        public static string InstrumentCatalog = ":INSTrument:CATalog?";

        /// <summary>Returns a list of logical instruments with their corresponding numbers (LINS).</summary>
        public static string InstrumentCatalogFull = ":INSTrument:CATalog:FULL?";

        /// <summary>Sets the internal system date (year, month, day).</summary>
        public static string SystemDate = ":SYSTem:DATE";

        /// <summary>Queries the internal system date.</summary>
        public static string SystemDateQuery = ":SYSTem:DATE?";

        /// <summary>Queries and removes the next error from the error queue.</summary>
        public static string SystemErrorNext = ":SYSTem:ERRor:NEXT?";

        /// <summary>Sets the internal system time (hour, minute, second).</summary>
        public static string SystemTime = ":SYSTem:TIME";

        /// <summary>Queries the internal system time.</summary>
        public static string SystemTimeQuery = ":SYSTem:TIME?";

        /// <summary>Returns the SCPI version number to which the instrument complies.</summary>
        public static string SystemVersionQuery = ":SYSTem:VERSion?";
    }
}
