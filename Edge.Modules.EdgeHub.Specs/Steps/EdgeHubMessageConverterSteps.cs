using System.Collections.Generic;
using System.Reflection;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using FluentAssertions;
using FluentAssertions.Json;
using System.Linq;
using RaaLabs.Edge.Testing;
using Autofac;
using BoDi;
using Moq;
using System;
using RaaLabs.Edge.Modules.EventHandling;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json.Linq;
using RaaLabs.Edge.Serialization;
using Autofac.Extras.Moq;
using RaaLabs.Edge.Modules.EdgeHub.Specs.Drivers;
using Microsoft.Azure.Devices.Client;
using System.Diagnostics.CodeAnalysis;

namespace RaaLabs.Edge.Modules.EdgeHub.Specs.Steps
{
    [Binding]
    class EdgeHubMessageConverterSteps
    {
        private readonly IObjectContainer _container;
        private readonly TypeMapping _typeMapping;
        private IEdgeHubMessageConverter _converter;

        public EdgeHubMessageConverterSteps(IObjectContainer container, TypeMapping typeMapping)
        {
            _container = container;
            _typeMapping = typeMapping;
        }

        [BeforeScenario]
        public void SetupEventFactories()
        {
            _container.RegisterTypeAs<SomeEdgeHubOutgoingEventInstanceFactory, IEventInstanceFactory<SomeEdgeHubOutgoingEvent>>();

            _container.RegisterTypeAs<SomeEdgeHubIncomingEventVerifier, IProducedEventVerifier<SomeEdgeHubIncomingEvent>>();
        }

        [Given(@"an EdgeHubMessageConverter with the following event types")]
        public void GivenAnIotHubMessageConverterWithTheFollowingEventTypes(Table table)
        {
            var mock = AutoMock.GetLoose(builder =>
            {
                builder.RegisterGeneric(typeof(JsonSerializer<>))
                    .AsSelf()
                    .As(typeof(ISerializer<>))
                    .SingleInstance();

                builder.RegisterGeneric(typeof(JsonDeserializer<>))
                    .AsSelf()
                    .As(typeof(IDeserializer<>))
                    .SingleInstance();
            });

            var eventTypes = table.Rows
                .Select(row => _typeMapping[row["Type"]])
                .ToList();

            var inputTypes = eventTypes.Where(type => type.IsAssignableTo<IEdgeHubIncomingEvent>()).ToHashSet();
            var outputTypes = eventTypes.Where(type => type.IsAssignableTo<IEdgeHubOutgoingEvent>()).ToHashSet();

            mock.Mock<IEventHandler<IEdgeHubIncomingEvent>>().Setup(handler => handler.GetSubtypes()).Returns(inputTypes);
            mock.Mock<IEventHandler<IEdgeHubOutgoingEvent>>().Setup(handler => handler.GetSubtypes()).Returns(outputTypes);

            _converter = mock.Create<EdgeHubMessageConverter>();
        }

        [Then(@"converting the following messages to events should give the expected result")]
        public void ThenConvertingTheFollowingMessagesToEventsShouldGiveTheExpectedResult(Table table)
        {
            var eventTypes = table.Rows.Select(row => _typeMapping[row["EventType"]]).Distinct();

            var verifiers = eventTypes
                .ToDictionary(type => type, type => (Action<IEvent, TableRow>)GetType().GetMethod("MakeEventVerifierMethod", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(type).Invoke(this, Array.Empty<object>()));

            foreach (var row in table.Rows)
            {
                var payload = Encoding.ASCII.GetBytes(row["Payload"]);
                var inputName = row["InputName"];
                var message = new Message(payload);
                var @event = _converter.ToEvent(inputName, message);
                var eventType = _typeMapping[row["EventType"]];
                verifiers[eventType](@event, row);
            }
        }

        [Then(@"converting the following events to messages should give the expected result")]
        public void ThenConvertingTheFollowingEventsToMessagesShouldGiveTheExpectedResult(Table table)
        {
            var eventTypes = table.Rows.Select(row => _typeMapping[row["EventType"]]).Distinct();

            var factories = eventTypes
                .ToDictionary(type => type, type => (Func<TableRow, IEvent>)GetType().GetMethod("MakeInstanceFactoryMethod", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(type).Invoke(this, Array.Empty<object>()));

            foreach (var row in table.Rows)
            {
                var eventType = _typeMapping[row["EventType"]];
                var @event = factories[eventType](row);
                var expectedOutputName = row["OutputName"];

                var (outputName, message) = _converter.ToMessage(@event) ?? (null, null);
                outputName.Should().Be(expectedOutputName);

                var expectedPayload = JObject.Parse(row["Payload"]);
                var actualPayload = JObject.Parse(Encoding.ASCII.GetString(message.GetBytes()));

                actualPayload.Should().BeEquivalentTo(expectedPayload);
            }
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called via reflection")]
        private Func<TableRow, IEvent> MakeInstanceFactoryMethod<T>() where T : class, IEvent
        {
            var factory = _container.Resolve<IEventInstanceFactory<T>>();

            return (row) => factory.FromTableRow(row);
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called via reflection")]
        private Action<IEvent, TableRow> MakeEventVerifierMethod<T>() where T : class, IEvent
        {
            var verifier = _container.Resolve<IProducedEventVerifier<T>>();

            return (@event, row) => verifier.VerifyFromTableRow(@event as T, row);
        }
    }
}
