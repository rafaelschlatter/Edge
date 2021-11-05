namespace RaaLabs.Edge.Modules.Timescaledb.Specs.Drivers
{
    public class SomeTimescaledbConnection : ITimescaledbConnection
    {
        public string ConnectionString { get; set; } = "SomeTimescaledbConnection";
    }

    public class AnotherTimescaledbConnection : ITimescaledbConnection
    {
        public string ConnectionString { get; set; } = "AnotherTimescaledbConnection";
    }

    [TimescaledbConnection(typeof(SomeTimescaledbConnection))]
    public class SomeTimescaledbEvent : ITimescaledbOutgoingEvent
    {
        public int Value { get; set; }
    }

    [TimescaledbConnection(typeof(AnotherTimescaledbConnection))]
    public class AnotherTimescaledbEvent : ITimescaledbOutgoingEvent
    {
        public string Value { get; set; }
    }

    [TimescaledbConnection(typeof(SomeTimescaledbConnection))]
    public class ThirdTimescaledbEvent : ITimescaledbOutgoingEvent
    {
        public float Value { get; set; }
    }
}
