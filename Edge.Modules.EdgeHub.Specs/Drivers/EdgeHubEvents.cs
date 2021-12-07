namespace RaaLabs.Edge.Modules.EdgeHub.Specs.Drivers
{

    [InputName("someinput")]
    public class SomeEdgeHubIncomingEvent : IEdgeHubIncomingEvent
    {
        public int Value { get; set; }
    }

    [InputName("anotherinput")]
    public class AnotherEdgeHubIncomingEvent : IEdgeHubIncomingEvent
    {
        public string Value { get; set; }
    }

    [OutputName("someoutput")]
    public class SomeEdgeHubOutgoingEvent : IEdgeHubOutgoingEvent
    {
        public int Value { get; set; }
    }

    [OutputName("anotheroutput")]
    public class AnotherEdgeHubOutgoingEvent : IEdgeHubOutgoingEvent
    {
        public string Value { get; set; }
    }
}
