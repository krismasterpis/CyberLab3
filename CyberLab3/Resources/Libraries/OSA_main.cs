using Microsoft.Extensions.Logging;
using ScpiNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberLab3.Resources.Libraries
{
    public class OSA : ScpiDevice
    {
		public static async Task<OSA> Create(IScpiConnection connection, ILogger<ScpiDevice> logger = null, CancellationToken cancellationToken = default)
        {
            // Try to open the connection:
            logger?.LogInformation($"Opening USB connection for device {connection.DevicePath}...");
            await connection.Open(cancellationToken);

            // Get device ID:
            logger?.LogInformation("Connection succeeded, trying to read device ID...");

            string id = await connection.GetId(cancellationToken);
            logger?.LogInformation($"Connection succeeded. Device id: {id}");

            // Create the driver instance.
            return new OSA(connection, id, logger);
        }

        /// <summary>
        /// The constructor is private, because we want to make the programmer to use the asynchronous factory method
        /// (constructors cannot be async).
        /// </summary>
        /// <param name="connection">Instance of connection to be used for communication.</param>
        /// <param name="deviceId">Devie ID.</param>
        /// <param name="logger">Logger instance.</param>
        protected OSA(IScpiConnection connection, string deviceId, ILogger<ScpiDevice> logger = null)
            : base(connection, deviceId, logger)
        {
        }

        /// <summary>
        /// This is how we can implement a driver-specific method.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Value of the device's status buffer.</returns>
        public async Task<string> ReadStatusByte(CancellationToken cancellationToken = default)
        {
            return await Query("*STB?", cancellationToken);
        }
    }
}
