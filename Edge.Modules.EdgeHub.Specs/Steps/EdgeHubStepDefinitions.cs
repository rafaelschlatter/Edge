using Autofac;
using TechTalk.SpecFlow;
using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RaaLabs.Edge.Modules.EdgeHub;
using Newtonsoft.Json;

namespace Edge.Modules.EventHandling.Specs.Steps
{
    [Binding]
    public sealed class EdgeHubStepDefinitions
    {

        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly ScenarioContext _scenarioContext;
        private ContainerBuilder _builder;
        private IContainer _container;
        private IDictionary<string, IntegerInputHandler> _integerInputHandlers;
        private IDictionary<string, IntegerOutputHandler> _integerOutputHandlers;

        public EdgeHubStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _integerInputHandlers = new Dictionary<string, IntegerInputHandler>();
            _integerOutputHandlers = new Dictionary<string, IntegerOutputHandler>();
        }

        [Given("an Autofac context")]
        public void GivenAnAutofacContext()
        {
            _builder = new ContainerBuilder();
        }

        [Given("module (.*) from (.*) is registered")]
        public void GivenNamedModuleIsRegistered(string moduleName, string assemblyName)
        {
            var moduleType = Type.GetType($"{moduleName}, {assemblyName}");
            var module = (Autofac.Core.IModule) Activator.CreateInstance(moduleType);
            _builder.RegisterModule(module);
        }

        [Given("IntegerInputHandler is registered")]
        public void GivenIntegerInputHandlerIsRegistered()
        {
            _builder.RegisterType<IntegerInputHandler>();
        }

        [Given("IntegerOutputHandler is registered")]
        public void GivenIntegerOutputHandlerIsRegistered()
        {
            _builder.RegisterType<IntegerOutputHandler>();
        }

        [Given("application container has been built")]
        public void GivenApplicationContainerHasBeenBuilt()
        {
            _container = _builder.Build();
        }

        [Given("an instance of IntegerInputHandler named (.*)")]
        public void GivenAnInstanceOfIntegerInputHandlerNamed(string instanceName)
        {
            _integerInputHandlers[instanceName] = _container.Resolve<IntegerInputHandler>();
        }

        [Given("an instance of IntegerOutputHandler named (.*)")]
        public void GivenAnInstanceOfIntegerOutputHandlerNamed(string instanceName)
        {
            _integerOutputHandlers[instanceName] = _container.Resolve<IntegerOutputHandler>();
        }

        [When("Building application container")]
        public void WhenBuildingContainer()
        {
            _container = _builder.Build();
        }

        [When("EdgeHub input IntegerInput receives (.*)")]
        public void WhenProducerProducesValue(string eventValue)
        {
            int value = int.Parse(eventValue);
            NullIotModuleClient client = (NullIotModuleClient) _container.Resolve<IIotModuleClient>();
            var incomingMessage = JsonConvert.SerializeObject(new EdgeHubIntegerInputEvent { Value = value });
            client.SimulateIncomingEvent("IntegerInput", incomingMessage);
        }

        [When("IntegerOutputHandler (.*) sends (.*)")]
        public void WhenOutputHandlerSendsValue(string instanceName, string eventValue)
        {
            int value = int.Parse(eventValue);
            _integerOutputHandlers[instanceName].Send(value);
        }

        [Then("Starting lifetime scope should succeed")]
        public void ThenBuildingScopeShouldSucceed()
        {
            using var scope = _container.BeginLifetimeScope();
        }

        [Then("IntegerInputHandler (.*) receives \\[(.*)\\]")]
        public void ThenConsumerReceivesEvent(string instanceName, string expectedValues)
        {
            var handler = _integerInputHandlers[instanceName];
            var expectedValuesList = expectedValues.Split(",").Select(_ => int.Parse(_)).ToList();
            handler.Values.Should().BeEquivalentTo(expectedValuesList);
        }

        [Then("EdgeHub output IntegerOutput sends \\[(.*)\\]")]
        public void ThenEdgeHubOutputIntegerOutputSendsValues(string expectedValues)
        {
            var expectedValuesList = expectedValues.Split(",").Select(_ => int.Parse(_)).ToList();
            NullIotModuleClient client = (NullIotModuleClient)_container.Resolve<IIotModuleClient>();
            var valuesSent = client.MessagesSent.Select(_ => JsonConvert.DeserializeObject<EdgeHubIntegerOutputEvent>(_).Value).ToList();
            expectedValuesList.Should().BeEquivalentTo(valuesSent);
        }
    }

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
        public void Handle(EdgeHubIntegerInputEvent @event)
        {
            Values.Add(@event.Value);
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
