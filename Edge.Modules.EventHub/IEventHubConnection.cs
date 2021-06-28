using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EventHub
{
    public interface IEventHubConnection
    {
        public string ConnectionString { get; set; }
        public string BlobStorageConnectionString { get; set; }
        public string BlobStorageContainerName { get; set; }
        public string EventHubName { get; set; }
        public string ConsumerGroup { get; set; }
        public int MaxIncomingBatchCount { get; set; }
    }
}
