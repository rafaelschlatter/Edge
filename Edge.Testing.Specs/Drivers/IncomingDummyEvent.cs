using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Testing.Specs.Drivers
{
    class IncomingDummyEvent : IEvent
    {
        public string Payload { get; set; }
    }
}
