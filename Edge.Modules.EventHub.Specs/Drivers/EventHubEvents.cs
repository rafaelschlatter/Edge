using Azure.Messaging.EventHubs.Primitives;

namespace RaaLabs.Edge.Modules.EventHub.Specs.Drivers
{
    public class IncomingEventHubConnection : IEventHubConnection
    {
        public string ConnectionString { get; set; } = "Hello!";
        public string EventHubName { get; set; } = "IncomingEventHub";
        public string ConsumerGroup { get; set; } = "Group1";
        public int MaxIncomingBatchCount { get; set; } = 10;
        public string BlobStorageConnectionString { get; set; } = "Helloooo";
        public string BlobStorageContainerName { get; set; } = "IncomingEventHubContainer";
        public bool DeleteCheckpointStoreAtStartup { get; set; } = false;
        public EventProcessorOptions ReaderOptions { get; set; } = new EventProcessorOptions();
    }

    public class OutgoingEventHubConnection : IEventHubConnection
    {
        public string ConnectionString { get; set; } = "Hello!";
        public string EventHubName { get; set; } = "OutgoingEventHub";
        public string ConsumerGroup { get; set; } = "Group1";
        public int MaxIncomingBatchCount { get; set; } = 10;
        public string BlobStorageConnectionString { get; set; } = "Helloooo";
        public string BlobStorageContainerName { get; set; } = "OutgoingEventHubContainer";
        public bool DeleteCheckpointStoreAtStartup { get; set; } = false;
        public EventProcessorOptions ReaderOptions { get; set; } = new EventProcessorOptions();
    }


    [EventHubConnection(typeof(IncomingEventHubConnection))]
    public class SomeEventHubIncomingEvent : IEventHubIncomingEvent
    {
        public int Value { get; set; }
    }

    [EventHubConnection(typeof(OutgoingEventHubConnection))]
    public class SomeEventHubOutgoingEvent : IEventHubOutgoingEvent
    {
        public int Value { get; set; }
    }
}
