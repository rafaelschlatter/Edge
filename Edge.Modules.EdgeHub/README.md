# EdgeHub
This document describes the EdgeHub module for the RaaLabs Edge framework.

## What does it do?
This module creates a bridge to the EdgeHub event broker, allowing you to map EdgeHub
inputs and outputs to EventHandling events.

## Concepts

### Incoming events
To receive a specific input from EdgeHub, you should create an event class implementing
the `IEdgeHubIncomingEvent` interface. This class should have the attribute
`InputName` set to the name of the input to listen to. The class will be used for
deserialization, so it should contain all properties to be deserialized from the incoming
payload.

The EdgeHub client will publish incoming events for the specified input, to the event
topic. Handlers will then be able to subscribe to this event like any other event.

Here is an example of an EdgeHub input, plus an event consumer for the input:
```csharp
[InputName("SomeIncomingEvent")]
class SomeIncomingEvent : IEdgeHubIncomingEvent
{
    public string Tag { get; set; }
    public string Value { get; set; }
}

class IncomingEventHandler : IConsumeEvent<SomeIncomingEvent>
{
    public void Handle(SomeIncomingEvent ev)
    {
        // Do something
    }
}
```

### Outgoing events
To send a specific input to EdgeHub, you should create an event class implementing
the `IEdgeHubOutgoingEvent` interface. This class should have the attribute
`OutputName` set to the name of the output to send to. The class will be used for
serialization, so it should contain all properties to be serialized to EdgeHub.

Handlers will be able to emit this event like any other. The EdgeHub client will
consume events from the event topic, to the specified output.

Here is an example of an EdgeHub output, plus an event producer for the output:
```csharp
[OutputName("SomeOutgoingEvent")]
class SomeOutgoingEvent : IEdgeHubOutgoingEvent
{
    public string Tag { get; set; }
    public string Value { get; set; }
}

class OutgoingEventProducer: IRunAsync, IProduceEvent<SomeOutgoingEvent>
{
    public event EventEmitter<SomeOutgoingEvent> SendEvent;

    public async Task Run()
    {
        while(true)
        {
            var ev = new SomeOutgoingEvent {
                Tag = "test",
                Value = "value"
            };
            SendEvent(ev);
            await Task.Delay(1000);
        }
    }
}
```
