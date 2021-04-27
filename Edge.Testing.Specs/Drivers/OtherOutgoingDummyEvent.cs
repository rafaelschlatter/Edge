using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Testing.Specs.Drivers
{
    class OtherOutgoingDummyEvent : IEvent
    {
        public string Payload { get; set; }
    }
}
