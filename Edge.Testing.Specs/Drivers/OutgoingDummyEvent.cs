using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Testing.Specs.Drivers
{
    class OutgoingDummyEvent : IEvent
    {
        public string Payload { get; set; }
    }
}
