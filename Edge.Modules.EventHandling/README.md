# Event Handling
This document describes the EventHandling module for the RaaLabs Edge framework.

## What does it do?
This module provides the application with an event handling backbone. This allows
components within the application to subscribe and consume events. This allows for
a loose coupling between components.

## Concepts

### Event class
This class is responsible for carrying the payload of the event. It also doubles as
the event "topic". All event classes must implement the `IEvent` interface, either
directly or indirectly through other interfaces.

Here is an example of an event class:
```csharp
class SomethingHappened : IEvent
{
    public string What { get; set; }
}
```

### Event Producer
When a class implements the `IProduceEvent<T>` interface, it tells the Event Handling
module that it produces events of the `T` event type. The Event Handling module will
expect the class to contain a delegate function with type signature `EventEmitter<T>`,
which will be called whenever a new event of type `T` is produced. If the Event Handling
module cannot find this delegate function, it will throw an exception and the application
will not start.

Here is an example of an event producer class:
```csharp
class SomethingHappenedProducer : IRunAsync, IProduceEvent<SomethingHappened>
{
    public event EventEmitter<SomethingHappened> SendSomethingHappenedEvent;

    public SomethingHappenedProducer()
    {
    }

    public async Task Run()
    {
        while(true)
        {
            var newEvent = new SomethingHappened
            {
                What = "Carrying on..."
            };
            SendSomethingHappenedEvent(newEvent);

            await Task.Delay(1000);
        }
    }
}
```

### Event Consumer
When a class implements the `IConsumeEvent<T>` interface, it tells the Event Handling
module that it consumes events of the `T` event type. This interface contains a
`void Handle(T ev)` function, which will be called in all consumers of an event type whenever
a new event of the given type is produced.

Here is an example of an event consumer class:
```csharp
class SomethingHappenedConsumer : IConsumeEvent<SomethingHappened>
{
    public void Handle(SomethingHappened ev)
    {
        // Process event here
    }
}
```

### Event Handler
Every event type has an Event Handler registered in the Autofac context, which is used internally
to handle the "plumbing" from an event is produced, until it is consumed. For normal use of the
framework, the developer can ignore this concept.
