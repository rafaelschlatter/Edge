# Configuration
This document describes the Configuration module for the RaaLabs Edge framework.

## What does it do?
This module eases loading of configuration files into the application.

## Concepts

### Configuration interface
Classes implementing this interface will be instantiated in the application by loading
the file specified in the `Name` attribute for the class. The properties of the class
will be used during deserialization.

Here is an example of a configuration file, a configuration class, and a class
depending on the configuration:

```json
{
  "someValue": "current value"
}
```

```csharp
[Name("someconfiguration.json")]
class SomeConfiguration : IConfiguration
{
    public string SomeValue { get; set; }
}

class SomeTask : IRunAsync
{
    private readonly SomeConfiguration _config;

    public SomeTask(SomeConfiguration config)
    {
        _config = config;
    }

    public async Task Run()
    {
        while(true)
        {
            Console.WriteLine($"Value: {_config.SomeValue}"});
            await Task.Delay(1000);
        }
    }
}
```