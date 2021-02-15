using Autofac;
using TechTalk.SpecFlow;
using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;

namespace Edge.Modules.EventHandling.Specs.Steps
{
    [Binding]
    public sealed class EventHandlingStepDefinitions
    {

        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly ScenarioContext _scenarioContext;
        private ContainerBuilder _builder;
        private IContainer _container;
        private IDictionary<string, (Type, Type)> _types;
        private IDictionary<string, (Type, object)> _instances;

        public EventHandlingStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _types = new Dictionary<string, (Type, Type)>();
            _instances = new Dictionary<string, (Type, object)>();
        }

        [Given("an Autofac context")]
        public void GivenAnAutofacContext()
        {
            _builder = new ContainerBuilder();
        }

        [Given("module (.*) from (.*) is registered")]
        public void GivenEventHandlingModuleIsRegistered(string moduleName, string assemblyName)
        {
            var moduleType = Type.GetType($"{moduleName}, {assemblyName}");
            var module = (Autofac.Core.IModule) Activator.CreateInstance(moduleType);
            _builder.RegisterModule(module);
        }

        [Given("a producer for (.*) named (.*)")]
        public void GivenAProducerForType(string typeAsString, string producerName)
        {
            var type = Type.GetType(typeAsString);
            var producerType = typeof(Producer<>).MakeGenericType(type);
            _builder.RegisterType(producerType).AsSelf();
            _types[producerName] = (type, producerType);
        }


        [Given("a consumer for (.*) named (.*)")]
        public void GivenAConsumerForType(string typeAsString, string consumerName)
        {
            var type = Type.GetType(typeAsString);
            var producerType = typeof(Consumer<>).MakeGenericType(type);
            _builder.RegisterType(producerType).AsSelf();
            _types[consumerName] = (type, producerType);
        }

        [Given("application container has been built")]
        public void GivenApplicationContainerHasBeenBuilt()
        {
            _container = _builder.Build();
        }

        [Given("an instance of (.*) named (.*)")]
        public void GivenAnInstanceOfTypeNamed(string typeAsString, string instanceName)
        {
            var (eventType, type) = _types[typeAsString];
            _instances[instanceName] = (eventType, _container.Resolve(type));
        }

        [When("Building application container")]
        public void WhenBuildingContainer()
        {
            _container = _builder.Build();
        }

        [When("(.*) produces (.*)")]
        public void WhenProducerProducesValue(string instanceName, string eventValue)
        {
            var (eventType, instance) = _instances[instanceName];
            instance.GetType().GetMethod("Produce").Invoke(instance, new object[] { Convert.ChangeType(eventValue, eventType) });

        }

        [Then("Starting lifetime scope should succeed")]
        public void ThenBuildingScopeShouldSucceed()
        {
            using var scope = _container.BeginLifetimeScope();
        }

        [Then("(.*) receives \\[(.*)\\]")]
        public void ThenConsumerReceivesEvent(string instanceName, string expectedValues)
        {
            var (eventType, instance) = _instances[instanceName];
            var events = instance.GetType().GetProperty("ReceivedEvents").GetValue(instance);
            var evs = Convert.ChangeType(events, typeof(List<>).MakeGenericType(eventType));
            var eventValueList = GetType().GetMethod("ListFromExpectedValuesString").MakeGenericMethod(eventType).Invoke(null, new object[] { expectedValues });
            GetType().GetMethod("AssertReceivedEvent").MakeGenericMethod(eventType).Invoke(null, new object[] { evs, eventValueList });
        }

        public static void AssertReceivedEvent<T>(List<T> actual, List<T> expected)
        {
            actual.Should().BeEquivalentTo(expected);
        }

        public static List<T> ListFromExpectedValuesString<T>(string expectedValues)
        {
            return expectedValues.Split(",").Select(v => v.Trim()).Select(v => (T) Convert.ChangeType(v, typeof(T))).ToList();
        }
    }

    class Producer<T> : IProduceEvent<Event<T>>
    {
        public event EventEmitter<Event<T>> ProduceEvent;

        public Producer()
        {

        }

        public void Produce(T value)
        {
            ProduceEvent(new Event<T>(value));
        }
    }

    class Consumer<T> : IConsumeEvent<Event<T>>
    {
        public List<T> ReceivedEvents { get; }
        public Consumer()
        {
            ReceivedEvents = new List<T>();
        }

        public void Handle(Event<T> @event)
        {
            ReceivedEvents.Add(@event.Payload);
        }
    }

    class Event<T> : IEvent
    {
        public T Payload { get; }
        public Event(T payload)
        {
            this.Payload = payload;
        }
    }
}
