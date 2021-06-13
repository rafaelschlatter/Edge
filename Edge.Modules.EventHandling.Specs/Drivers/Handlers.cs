using Autofac;
using TechTalk.SpecFlow;
using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EventHandling.Specs.Drivers
{
    interface IProducer
    {
        public void Produce(int value);
    }

    interface IAsyncProducer
    {
        public Task Produce(int value);
    }

    class Producer : IProduceEvent<Event>, IProducer
    {
        public event EventEmitter<Event> ProduceEvent;

        public Producer()
        {

        }

        public void Produce(int value)
        {
            ProduceEvent(new Event(value));
        }
    }

    class AsyncProducer : IProduceEvent<Event>, IAsyncProducer
    {
        public event AsyncEventEmitter<Event> ProduceEvent;

        public AsyncProducer()
        {

        }

        public async Task Produce(int value)
        {
            await ProduceEvent(new Event(value));
        }
    }

    interface IConsumer
    {
        public List<int> ReceivedEvents { get; }

    }

    class Consumer : IConsumeEvent<Event>, IConsumer
    {
        public List<int> ReceivedEvents { get; }
        public Consumer()
        {
            ReceivedEvents = new List<int>();
        }

        public virtual void Handle(Event @event)
        {
            ReceivedEvents.Add(@event.Payload);
        }
    }

    class SquaringConsumer : Consumer, IConsumeEvent<Event>, IConsumer
    {
        public SquaringConsumer() : base()
        {
        }

        public override void Handle(Event @event)
        {
            ReceivedEvents.Add(@event.Payload * @event.Payload);
        }
    }

    class AsyncConsumer : IConsumeEventAsync<Event>, IConsumer
    {
        public List<int> ReceivedEvents { get; }
        public AsyncConsumer()
        {
            ReceivedEvents = new List<int>();
        }

        public async Task HandleAsync(Event @event)
        {
            ReceivedEvents.Add(@event.Payload);
            await Task.CompletedTask;
        }
    }

    class Event : IEvent
    {
        public int Payload { get; }
        public Event(int payload)
        {
            this.Payload = payload;
        }
    }
}
