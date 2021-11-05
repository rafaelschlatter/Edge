namespace RaaLabs.Edge.Modules.IotHub.Specs.Drivers
{
    public class IotHubConnection : IIotHubConnection
    {
        public string ConnectionString { get; set; } = "Hello!";
        public string IotHubName { get; set; } = "SomeIotHub";
        public string ConsumerGroup { get; set; } = "Group1";
        public int MaxIncomingBatchCount { get; set; } = 10;
    }


    [IotHubConnection(typeof(IotHubConnection))]
    public class SomeIotHubIncomingEvent : IIotHubIncomingEvent
    {
        public int Value { get; set; }
    }

    [IotHubConnection(typeof(IotHubConnection))]
    public class SomeIotHubOutgoingEvent : IIotHubOutgoingEvent
    {
        public int Value { get; set; }
    }
}
