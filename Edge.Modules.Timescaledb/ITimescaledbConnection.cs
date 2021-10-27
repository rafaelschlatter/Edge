using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.Timescaledb
{
    public interface ITimescaledbConnection : IClientConnection
    {
        public string ConnectionString { get; set; }
    }
}
