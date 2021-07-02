using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.IotHub
{
    public interface IIotHubConnection
    {
        public string ConnectionString { get; set; }
        public string IotHubName { get; set; }
        public string ConsumerGroup { get; set; }
        public int MaxIncomingBatchCount { get; set; }
    }
}
