using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Testing.Specs.Drivers
{
    class DummyHandler : IConsumeEvent<IncomingDummyEvent>, IProduceEvent<OutgoingDummyEvent>, IProduceEvent<OtherOutgoingDummyEvent>
    {
        public event EventEmitter<OutgoingDummyEvent> SendDummyEvent;
        public event EventEmitter<OtherOutgoingDummyEvent> SendOtherDummyEvent;

        public void Handle(IncomingDummyEvent @event)
        {
            var outgoingDummyEvent = new OutgoingDummyEvent { Payload = $"{@event.Payload}{@event.Payload}" };
            var otherOutgoingDummyEvent = new OtherOutgoingDummyEvent { Payload = $"{@event.Payload}{@event.Payload}" };
            SendDummyEvent(outgoingDummyEvent);
            SendOtherDummyEvent(otherOutgoingDummyEvent);
        }
    }
}
