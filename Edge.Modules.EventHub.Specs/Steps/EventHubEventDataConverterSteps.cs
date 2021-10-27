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
using RaaLabs.Edge.Modules.EventHub.Specs.Drivers;
using Microsoft.Azure.Devices.Client;
using Azure.Messaging.EventHubs;
using System.Diagnostics.CodeAnalysis;

namespace RaaLabs.Edge.Modules.EventHub.Specs.Steps
{
    [Binding]
    class EventHubEventDataConverterSteps
    {
        private readonly IObjectContainer _container;
        private readonly TypeMapping _typeMapping;
        private IEventHubEventDataConverter _converter;

        public EventHubEventDataConverterSteps(IObjectContainer container, TypeMapping typeMapping)
        {
            _container = container;
            _typeMapping = typeMapping;
        }

        [BeforeScenario]
        public void SetupEventFactories()
        {
            _container.RegisterTypeAs<SomeEventHubIncomingEventInstanceFactory, IEventInstanceFactory<SomeEventHubIncomingEvent>>();
            _container.RegisterTypeAs<SomeEventHubOutgoingEventInstanceFactory, IEventInstanceFactory<SomeEventHubOutgoingEvent>>();

            _container.RegisterTypeAs<SomeEventHubIncomingEventVerifier, IProducedEventVerifier<SomeEventHubIncomingEvent>>();
            _container.RegisterTypeAs<SomeEventHubOutgoingEventVerifier, IProducedEventVerifier<SomeEventHubOutgoingEvent>>();
        }

        [Given(@"an EventHubEventDataConverter with the following event types")]
        public void GivenAnEventHubEventDataConverterWithTheFollowingEventTypes(Table table)
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

            var inputTypes = eventTypes.Where(type => type.IsAssignableTo<IEventHubIncomingEvent>()).ToHashSet();
            var outputTypes = eventTypes.Where(type => type.IsAssignableTo<IEventHubOutgoingEvent>()).ToHashSet();

            mock.Mock<IEventHandler<IEventHubIncomingEvent>>().Setup(handler => handler.GetSubtypes()).Returns(inputTypes);
            mock.Mock<IEventHandler<IEventHubOutgoingEvent>>().Setup(handler => handler.GetSubtypes()).Returns(outputTypes);

            _converter = mock.Create<EventHubEventDataConverter>();
        }

        [Then(@"converting the following EventData to events should give the expected result")]
        public void ThenConvertingTheFollowingEventDataToEventsShouldGiveTheExpectedResult(Table table)
        {
            var eventTypes = table.Rows.Select(row => _typeMapping[row["EventType"]]).Distinct();

            var verifiers = eventTypes
                .ToDictionary(type => type, type => (Action<IEvent, TableRow>)GetType().GetMethod("MakeEventVerifierMethod", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(type).Invoke(this, Array.Empty<object>()));

            foreach (var row in table.Rows)
            {
                var payload = Encoding.ASCII.GetBytes(row["Payload"]);
                var connection = _typeMapping[row["Connection"]];
                var data = new EventData(payload);
                var @event = _converter.ToEvent(connection, data);
                var eventType = _typeMapping[row["EventType"]];
                verifiers[eventType](@event, row);
            }
        }

        [Then(@"converting the following events to EventData should give the expected result")]
        public void ThenConvertingTheFollowingEventsToEventDataShouldGiveTheExpectedResult(Table table)
        {
            var eventTypes = table.Rows.Select(row => _typeMapping[row["EventType"]]).Distinct();

            var factories = eventTypes
                .ToDictionary(type => type, type => (Func<TableRow, IEvent>)GetType().GetMethod("MakeInstanceFactoryMethod", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(type).Invoke(this, Array.Empty<object>()));

            foreach (var row in table.Rows)
            {
                var type = _typeMapping[row["EventType"]];
                var @event = factories[type](row);
                var expectedConnection = _typeMapping[row["Connection"]];

                var (connection, data) = _converter.ToEventData(@event) ?? (null, null);
                connection.Should().Be(expectedConnection);

                var expectedPayload = JObject.Parse(row["Payload"]);
                var actualPayload = JObject.Parse(Encoding.ASCII.GetString(data.EventBody.ToArray()));

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
