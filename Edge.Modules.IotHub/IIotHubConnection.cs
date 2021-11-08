using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.IotHub
{
    public interface IIotHubConnection : IClientConnection
    {
        public string ConnectionString { get; set; }
        public string IotHubName { get; set; }
        public string ConsumerGroup { get; set; }
        public int MaxIncomingBatchCount { get; set; }
    }
}
