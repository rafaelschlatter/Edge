# IotHub
This document describes the IotHub module for the RaaLabs Edge framework.

## What does it do?
This module creates a bridge to the IotHub event broker, allowing you to map IotHub
inputs and outputs to EventHandling events.

## Configuration
All IotHubs to connect to should have an environment variable "<IOT_HUB_NAME>_CONNECTION_STRING"
set to a valid connection string for that IotHub.

## Concepts

### Incoming events
To receive a specific input from IotHub, you should create an event class implementing
the `IIotHubIncomingEvent` interface. This class should have the attribute
`IotHubName` set to the name of the IotHub to listen to. The class will be used for
deserialization, so it should contain all properties to be deserialized from the incoming
payload.

The IotHub client will publish incoming events for the specified input, to the event
topic. Handlers will then be able to subscribe to this event like any other event.

Here is an example of an IotHub input, plus an event consumer for the input:
```csharp
[IotHubName("iothubone")]
class SomeIncomingEvent : IIotHubIncomingEvent
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
To send a specific input to IotHub, you should create an event class implementing
the `IIotHubOutgoingEvent` interface. This class should have the attribute
`IotHubName` set to the name of the IotHub to send to. The class will be used for
serialization, so it should contain all properties to be serialized to IotHub.

Handlers will be able to emit this event like any other. The IotHub client will
consume events from the event topic, to the specified output.

Here is an example of an IotHub output, plus an event producer for the output:
```csharp
[IotHubName("iothubone")]
class SomeOutgoingEvent : IIotHubOutgoingEvent
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
