using System;

namespace RaaLabs.Edge.Modules.EdgeHub
{
    public class IotEdgeHelpers
    {
        /// <summary>
        /// Check if we're running in IoT Edge context or not
        /// </summary>
        /// <returns>True if we are running in IoT Edge context, false if not</returns>
        public static bool IsRunningInIotEdge()
        {
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("IOTEDGE_MODULEID"));
        }
    }
}
