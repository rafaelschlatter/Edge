using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.Configuration
{
    [ExcludeFromCodeCoverage]
    public class ApplicationRestartTrigger : IApplicationRestartTrigger
    {
        public void RestartApplication()
        {
            Environment.Exit(0);
        }
    }
}
