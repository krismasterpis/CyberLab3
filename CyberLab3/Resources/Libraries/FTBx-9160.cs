using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberLab3.Resources.Libraries
{
    public class FTBx_9160
    {

        private EXFO_LTB8 _parentDevice;

        // Instrument prefix (must be added if controlling a specific module in a platform)
        // {0} = Logical Instrument Position
        public const string CMD_INSTRUMENT_PREFIX = "LINStrument{0}:";

        // API Lock Commands
        public const string CMD_LOCK_STATE_ON = ":LOCK:STATe ON";
        public const string CMD_LOCK_STATE_OFF = ":LOCK:STATe OFF";
        public const string CMD_LOCK_STATE_QUERY = ":LOCK:STATe?";

        // Route / Path Control Commands
        public const string CMD_ROUTE_CLOSE = ":ROUTe:CLOSe";         // Sets switch to reset position (no continuity)
        public const string CMD_ROUTE_OPEN = ":ROUTe:OPEN";           // Restores connection to the last channel
        public const string CMD_ROUTE_OPEN_STATE_QUERY = ":ROUTe:OPEN:STATe?"; // Checks continuity (0/1)
        public const string CMD_ROUTE_PATH_CATALOG_QUERY = ":ROUTe:PATH:CATalog?"; // Returns switch type (e.g., "1x12")

        // Channel Scanning / Selection
        public const string CMD_ROUTE_SCAN = ":ROUTe:SCAN {0}";       // Sets channel {0} (e.g., 1, 2...)
        public const string CMD_ROUTE_SCAN_QUERY = ":ROUTe:SCAN?";    // Returns current channel
        public const string CMD_ROUTE_SCAN_NEXT = ":ROUTe:SCAN:NEXT"; // Move to next channel
        public const string CMD_ROUTE_SCAN_PREV = ":ROUTe:SCAN:PREV"; // Move to previous channel

        // Precision and Repeatability Configuration
        public const string CMD_ROUTE_SCAN_ADJUST = ":ROUTe:SCAN:ADJust"; // Forces return to reference position
        public const string CMD_ROUTE_SCAN_ADJUST_AUTO = ":ROUTe:SCAN:ADJust:AUTO {0}"; // {0} = 1 (ON) or 0 (OFF)
        public const string CMD_ROUTE_SCAN_ADJUST_AUTO_QUERY = ":ROUTe:SCAN:ADJust:AUTO?";

        // Synchronous Mode
        public const string CMD_ROUTE_SCAN_SYNC = ":ROUTe:SCAN:SYNChronous {0}"; // {0} = 1 (ON) or 0 (OFF)
        public const string CMD_ROUTE_SCAN_SYNC_QUERY = ":ROUTe:SCAN:SYNChronous?";

        // General Common Commands
        public const string CMD_RESET = ":RST";         // Resets the device
        public const string CMD_SERIAL_NUMBER_QUERY = ":SNUMber?"; // Returns serial number
        public const string CMD_STATUS_QUERY = ":STATus?";         // Returns status (READY, BUSY, etc.)

        // ---------------------------------------------------------
        // Fields and Communication Interface
        // ---------------------------------------------------------

        private readonly int _logicalInstrumentPosition;

        /// <summary>
        /// Constructor for the driver class.
        /// </summary>
        /// <param name="logicalInstrumentPosition">Logical instrument position in the platform (e.g., 1).</param>
        public FTBx_9160(EXFO_LTB8 parentDevice,int logicalInstrumentPosition = 1)
        {
            _logicalInstrumentPosition = logicalInstrumentPosition;
            _parentDevice = parentDevice;
        }

        // ---------------------------------------------------------
        // Control Methods (Switches / Logic)
        // ---------------------------------------------------------

        /// <summary>
        /// Sets the switch to a specific channel (port).
        /// Corresponds to command: :ROUTe:SCAN <Position>
        /// </summary>
        /// <param name="channel">Channel number (1-N for 1xN switches).</param>
        public void SetChannel(int channel)
        {
            string command = string.Format(CMD_ROUTE_SCAN, channel);
            _parentDevice.SendCommand(command);
        }

        /// <summary>
        /// Gets the currently active channel.
        /// Corresponds to command: :ROUTe:SCAN?
        /// </summary>
        /// <returns>Active channel number.</returns>
        public int GetChannel()
        {
            string response = _parentDevice.Query(CMD_ROUTE_SCAN_QUERY);
            if (int.TryParse(response, out int channel))
            {
                return channel;
            }
            throw new Exception($"Invalid device response: {response}");
        }

        /// <summary>
        /// Disconnects the optical path (no continuity).
        /// Corresponds to command: :ROUTe:CLOSe
        /// </summary>
        public void DisconnectOpticalPath()
        {
            _parentDevice.SendCommand(CMD_ROUTE_CLOSE);
        }

        /// <summary>
        /// Restores the optical connection (last selected channel).
        /// Corresponds to command: :ROUTe:OPEN
        /// </summary>
        public void ConnectOpticalPath()
        {
            _parentDevice.SendCommand(CMD_ROUTE_OPEN);
        }

        /// <summary>
        /// Checks if the optical path is connected.
        /// Corresponds to command: :ROUTe:OPEN:STATe?
        /// </summary>
        /// <returns>True if connected, False if disconnected.</returns>
        public bool IsOpticalPathConnected()
        {
            string response = _parentDevice.Query(CMD_ROUTE_OPEN_STATE_QUERY);
            // Documentation: 1 = optical continuity, 0 = no continuity
            return response.Trim() == "1";
        }

        /// <summary>
        /// Switches to the next channel.
        /// </summary>
        public void NextChannel()
        {
            _parentDevice.SendCommand(CMD_ROUTE_SCAN_NEXT);
        }

        /// <summary>
        /// Switches to the previous channel.
        /// </summary>
        public void PreviousChannel()
        {
            _parentDevice.SendCommand(CMD_ROUTE_SCAN_PREV);
        }

        /// <summary>
        /// Configures "High Repeatability" mode.
        /// If enabled, the switch returns to a reference position before changing channels.
        /// </summary>
        /// <param name="enable">True to enable, False to disable.</param>
        public void SetHighRepeatability(bool enable)
        {
            string param = enable ? "1" : "0";
            string command = string.Format(CMD_ROUTE_SCAN_ADJUST_AUTO, param);
            _parentDevice.SendCommand(command);
        }

        /// <summary>
        /// Gets the device status (e.g., READY, BUSY).
        /// </summary>
        public string GetStatus()
        {
            return _parentDevice.Query(CMD_STATUS_QUERY).Trim();
        }

        /// <summary>
        /// Gets the module serial number.
        /// </summary>
        public string GetSerialNumber()
        {
            return _parentDevice.Query(CMD_SERIAL_NUMBER_QUERY).Trim();
        }

        /// <summary>
        /// Resets the device to default settings.
        /// </summary>
        public void Reset()
        {
            _parentDevice.SendCommand(CMD_RESET);
        }

        // ---------------------------------------------------------
        // Helper Methods
        // ---------------------------------------------------------

        /// <summary>
        /// Prepends the LINStrument<n>: prefix required by the EXFO platform.
        /// </summary>
        private string BuildCommand(string cmd)
        {
            string prefix = string.Format(CMD_INSTRUMENT_PREFIX, _logicalInstrumentPosition);
            return $"{prefix}{cmd}";
        }
    }

    /// <summary>
    /// Interface for the transport layer (e.g., TCP/IP, VISA).
    /// Implement this based on your connection medium.
    /// </summary>
    public interface IScpiConnection
    {
        void Write(string command);
        string Query(string command);
    }
}
