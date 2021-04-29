using System;
using System.Diagnostics.CodeAnalysis;

namespace RaaLabs.Edge.Modules.Configuration
{
    /// <summary>
    /// A "power-off button" for the application
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ApplicationShutdownTrigger : IApplicationShutdownTrigger
    {
        /// <summary>
        /// Will shut down the application
        /// </summary>
        public void ShutdownApplication()
        {
            Environment.Exit(0);
        }
    }
}
