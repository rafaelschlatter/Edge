using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EventHandling
{
    public interface IBridge
    {
        public Task SetupBridge();
    }

    public interface IBridgeIncomingEvent<T> : IBridge, IProduceEvent<T>
    where T : IEvent
    {

    }

    public interface IBridgeOutgoingEvent<in T> : IBridge, IConsumeEvent<T>
    where T : IEvent
    {

    }
}
