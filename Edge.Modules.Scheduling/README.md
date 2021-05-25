# Scheduling
This document describes the Scheduling module for the RaaLabs Edge framework.

## What does it do?
This module provides the application with the ability to schedule events through
the Event Handling system.

## How to use
A scheduled event must implement the `IScheduledEvent` interface.

There are two ways of configuring a scheduled event:
1. Adding a `Schedule` attribute to the class definition
2. Adding a type implementing the `IScheduleForType<T>` interface, where T is the event type

### Configuration using `Schedule` attribute
The schedule attribute is a simple way to configure the event to trigger. The attribute 
can contain either a scheduling pattern (using cron format), or a scheduling interval.

Here is an example of an event triggered with a cron pattern:
```csharp
[Schedule(Pattern = "*/1 * * * * ?")]
public class CronEvent : IScheduledEvent
{
}
```

Here is an example of an event triggered with an interval of 0.25 (four times a second):
```csharp
[Schedule(Interval = 0.25)]
public class CronEvent : IScheduledEvent
{
}
```

### Configuration using `IScheduleForType<T>` interface
This way of setting up scheduling allows for more advanced configuration, like for example
configuring schedules from a configuration file. It is also currently the only schedule
setup type that allows schedule events with payloads.

A class implementing `IScheduleForType<T>` must implement the `Schedules` property,
which will return a dictionary containing the actual schedules. Each entry in this dictionary
can be either a `Interval<T>` or a `Pattern<T>` type, each containing required information for the
schedule type.

Here is a full example for using this type of schedule setup:
```csharp
public class ScheduleConfig : IScheduleForType<TypeScheduledEvent>
{
    private Dictionary<string, ISchedule> _schedules;

    // by depending on a configuration file here, you could build up _schedules
    // from the fields in the configuration file.
    public ScheduleConfig()
    {
        _schedules = new Dictionary<string, ISchedule>
        {
            { "onceASecond", new Pattern<TypeScheduledEvent>{ Value = "*/1 * * * * ?", Payload = new TypeScheduledEvent { Tags = new List<string>{ "tag-1", "tag-2" } } } },
            { "fourTimesASecond", new Interval<TypeScheduledEvent>{ Value = 0.25, Payload = new TypeScheduledEvent { Tags = new List<string>{ "tag-3", "tag-4" } } } }
        };
    }

    public Dictionary<string, ISchedule> Schedules => _schedules;
}

public class TypeScheduledEvent : IScheduledEvent
{
    public List<string> Tags { get; set; }
}
```
