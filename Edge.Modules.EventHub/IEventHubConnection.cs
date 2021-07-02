using Azure.Messaging.EventHubs.Primitives;

namespace RaaLabs.Edge.Modules.EventHub
{
    public interface IEventHubConnection
    {
        public string EventHubName { get; set; }
        public string ConnectionString { get; set; }
        public string ConsumerGroup { get; set; }
        public int MaxIncomingBatchCount { get; set; }
        public string BlobStorageConnectionString { get; set; }
        public string BlobStorageContainerName { get; set; }
        public bool DeleteCheckpointStoreAtStartup { get; set; }
        public EventProcessorOptions Options { get; set;}
    }
}
