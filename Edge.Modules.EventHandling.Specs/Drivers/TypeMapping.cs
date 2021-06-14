using Edge.Modules.EventHandling.Specs.Steps;
using RaaLabs.Edge.Modules.EventHandling.Specs.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EventHandling.Specs.Drivers
{
    public class TypeMapping : Dictionary<string, Type>
    {
        public TypeMapping()
        {
            Add("EventHandling", typeof(EventHandling));
            Add("Producer", typeof(Producer));
            Add("AsyncProducer", typeof(AsyncProducer));
            Add("Consumer", typeof(Consumer));
            Add("SquaringConsumer", typeof(SquaringConsumer));
            Add("AsyncConsumer", typeof(AsyncConsumer));
            Add("Event", typeof(Event));
        }
    }
}
