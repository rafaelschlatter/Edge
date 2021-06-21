# EventHub
This document describes the EventHub module for the RaaLabs Edge framework.

## What does it do?
This module creates a bridge to the EventHub event broker, allowing you to map EventHub
inputs and outputs to EventHandling events.

## Configuration
All EventHubs to connect to should have an environment variable "<EVENT_HUB_NAME>_CONNECTION_STRING"
set to a valid connection string for that EventHub.

## Concepts

### Incoming events
To receive a specific input from EventHub, you should create an event class implementing
the `IEventHubIncomingEvent` interface. This class should have the attribute
`EdgeHubName` set to the name of the EdgeHub to listen to. The class will be used for
deserialization, so it should contain all properties to be deserialized from the incoming
payload.

The EventHub client will publish incoming events for the specified input, to the event
topic. Handlers will then be able to subscribe to this event like any other event.

Here is an example of an EventHub input, plus an event consumer for the input:
```csharp
[EventHubName("eventhubone")]
class SomeIncomingEvent : IEventHubIncomingEvent
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
To send a specific input to EventHub, you should create an event class implementing
the `IEventHubOutgoingEvent` interface. This class should have the attribute
`EventHubName` set to the name of the EventHub to send to. The class will be used for
serialization, so it should contain all properties to be serialized to EventHub.

Handlers will be able to emit this event like any other. The EventHub client will
consume events from the event topic, to the specified output.

Here is an example of an EventHub output, plus an event producer for the output:
```csharp
[EventHubName("eventhubone")]
class SomeOutgoingEvent : IEventHubOutgoingEvent
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
