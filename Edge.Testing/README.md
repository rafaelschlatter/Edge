# EdgeHub Testing
This document describes the Testing module for the RaaLabs Edge framework.

## What does it do?
This module provides the developer with common functionality useful across
projects. This includes Specflow bindings for testing handlers.

## Handler bindings
The HandlerSteps class provides bindings to SpecFlow for testing the mapping
from input events to output events for a given handler class. The setup is
a bit extensive, so we'll go through all the requirements here:

### SpecFlow configuration file (specflow.json)
Your must append RaaLabs.Edge.Testing to the list of external assemblies in specflow.json.

Example:

```json
{
    "allowDebugGeneratedFiles": true,
    "stepAssemblies": [
        {
            "assembly": "RaaLabs.Edge.Testing"
        }
    ]
}

```

### Assembly registration
For component scanning to work, you need to specify the assembly .dll file where the
classes for the test is located. This can easily be done by adding the following class
to your test project:

```csharp
[Binding]
class AssemblyRegistration
{
    private readonly ComponentAssemblies _assemblies;

    public AssemblyRegistration(ComponentAssemblies assemblies)
    {
        _assemblies = assemblies;
    }

    [BeforeScenario]
    private void RegisterAssembly()
    {
        _assemblies.Add(GetType().Assembly);
    }
}
```

### TypeMapping
You need to provide string representations of your handler and event classes,
in the `TypeMapping` dictionary.
This should be done in a SpecFlow "Hook", like this:

```csharp
[Binding]
public sealed class TypeMapperProvider
{
    private readonly TypeMapping _typeMapping;

    public TypeMapperProvider(TypeMapping typeMapping)
    {
        _typeMapping = typeMapping;
    }

    [BeforeScenario]
    public void SetupTypes()
    {
        _typeMapping.Add("MyIncomingEventHandler", typeof(MyIncomingEventHandler));
        _typeMapping.Add("IncomingEvent", typeof(Events.IncomingEvent));
        _typeMapping.Add("OutgoingEvent", typeof(Events.OutgoingEvent));
    }
}
```

### EventInstanceFactory
All events that will be produced needs to have an `IEventInstanceFactory` associated with it.
This class will provide functionality to create instances of the given event type from
table rows in the SpecFlow feature file.
You need to create a class implementing `IEventInstanceFactory<T>`, with `T` being the type
of your incoming event.

The Edge.Testing project will automatically register all classes implementing `IEventInstanceFactory<>`.

Example:

```csharp
class IncomingEventInstanceFactory : IEventInstanceFactory<IncomingEvent>
{
    public IncomingEvent FromTableRow(TableRow row)
    {
        var value = float.Parse(row["Value"]);
        return new IncomingEvent
        {
            Value = value
        };
    }
}
```

### EventVerifier
To verify that the events produced by a handler class are correctly produced, the outgoing
events needs to have an `IProducedEventVerifier` associated with it. This class wil provide
functionality to verify each produced event with rows from a table in the SpecFlow feature file.
You need to create a class implementing `IProducedEventVerifier<T>`, with `T` being the type
of your outgoing event.

The Edge.Testing project will automatically register all classes implementing `IProducedEventVerifier<>`.

Example:

```csharp
class OutgoingEventVerifier : IProducedEventVerifier<OutgoingEvent>
{
    public void VerifyFromTableRow(OutgoingEvent @event, TableRow row)
    {
        @event.value.Should().BeApproximately(float.Parse(row["Value"]), 0.0001f);
    }
}
```