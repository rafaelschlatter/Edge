using Autofac;
using TechTalk.SpecFlow;
using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RaaLabs.Edge.Modules.EdgeHub;
using Newtonsoft.Json;

namespace RaaLabs.Edge.Modules.EventHandling.Specs.Drivers
{

    [InputName("IntegerInput")]
    class EdgeHubIntegerInputEvent : IEdgeHubIncomingEvent
    {
        public int Value { get; set; }
    }

    [OutputName("IntegerOutput")]
    class EdgeHubIntegerOutputEvent : IEdgeHubOutgoingEvent
    {
        public int Value { get; set; }
    }

    class IntegerInputHandler : IConsumeEvent<EdgeHubIntegerInputEvent>
    {
        public List<int> Values { get; }
        public IntegerInputHandler()
        {
            Values = new List<int>();
        }
        public virtual void Handle(EdgeHubIntegerInputEvent @event)
        {
            Values.Add(@event.Value);
        }
    }

    class IntegerInputSquaringHandler : IntegerInputHandler, IConsumeEvent<EdgeHubIntegerInputEvent>
    {
        public IntegerInputSquaringHandler()
            : base()
        {
        }
        public override void Handle(EdgeHubIntegerInputEvent @event)
        {
            Values.Add(@event.Value * @event.Value);
        }
    }

    class IntegerOutputHandler : IProduceEvent<EdgeHubIntegerOutputEvent>
    {
        public event EventEmitter<EdgeHubIntegerOutputEvent> SendData;
        public IntegerOutputHandler()
        {
        }

        public void Send(int value)
        {
            SendData(new EdgeHubIntegerOutputEvent { Value = value });
        }
    }
}
