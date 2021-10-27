using RaaLabs.Edge.Modules.Mqtt;
using System.Collections.Generic;
using System.Reflection;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using FluentAssertions;
using System.Linq;
using RaaLabs.Edge.Testing;
using Autofac;
using BoDi;
using RaaLabs.Edge.Modules.Mqtt.Client;
using RaaLabs.Edge.Modules.Mqtt.Specs.Drivers;
using Moq;
using System;
using RaaLabs.Edge.Modules.EventHandling;
using MQTTnet;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json.Linq;
using RaaLabs.Edge.Modules.Mqtt.Client.Authentication;
using RaaLabs.Edge.Serialization;
using Autofac.Extras.Moq;

namespace RaaLabs.Edge.Modules.Mqtt.Specs.Steps
{
    [Binding]
    class MqttMessageConverterSteps
    {
        private readonly IObjectContainer _container;
        private TypeMapping _typeMapping;
        private IMqttMessageConverter _converter;
        private Mock<IMqttTopicMapper> _topicMapper;

        public MqttMessageConverterSteps(IObjectContainer container, TypeMapping typeMapping)
        {
            _container = container;
            _typeMapping = typeMapping;
        }

        [BeforeScenario]
        public void SetupEventFactories()
        {
            _container.RegisterTypeAs<SomeMqttIncomingEventInstanceFactory, IEventInstanceFactory<SomeMqttIncomingEvent>>();
            _container.RegisterTypeAs<AnotherMqttIncomingEventInstanceFactory, IEventInstanceFactory<AnotherMqttIncomingEvent>>();

            _container.RegisterTypeAs<SomeMqttOutgoingEventVerifier, IProducedEventVerifier<SomeMqttOutgoingEvent>>();
            _container.RegisterTypeAs<AnotherMqttOutgoingEventVerifier, IProducedEventVerifier<AnotherMqttOutgoingEvent>>();
        }

        [Given(@"an MqttMessageConverter with the following event types")]
        public void GivenAnMqttMessageConverter(Table table)
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

            var inputTypes = eventTypes.Where(type => type.IsAssignableTo<IMqttIncomingEvent>()).ToHashSet();
            var outputTypes = eventTypes.Where(type => type.IsAssignableTo<IMqttOutgoingEvent>()).ToHashSet();

            mock.Mock<IEventHandler<IMqttIncomingEvent>>().Setup(handler => handler.GetSubtypes()).Returns(inputTypes);
            mock.Mock<IEventHandler<IMqttOutgoingEvent>>().Setup(handler => handler.GetSubtypes()).Returns(outputTypes);

            _topicMapper = mock.Mock<IMqttTopicMapper>();

            _converter = mock.Create<MqttMessageConverter>();
        }

        [Given(@"a mocked TopicMapper with the following rules")]
        void GivenAMockedTopicMapperWithTheFollowingRules(Table table)
        {
            foreach (var row in table.Rows)
            {
                var connection = _typeMapping[row["Connection"]];
                var pattern = row["Route"];
                var target = _typeMapping[row["Target"]];
                _topicMapper.Setup(mapper => mapper.Resolve(connection, It.IsRegex(pattern)))
                    .Returns(target);
            }
        }

        [Then(@"converting the following events to messages and back should succeed")]
        void ThenConvertingTheFollowingEventsToMessagesAndBackShouldSucceed(Table table)
        {
            var eventTypes = table.Rows.Select(row => _typeMapping[row["Type"]]).Distinct();
            var factories = eventTypes
                .ToDictionary(type => type, type => (Func<TableRow, IEvent>)GetType().GetMethod("MakeInstanceFactoryMethod", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(type).Invoke(this, new object[] { }));

            var verifiers = eventTypes
                .ToDictionary(type => type, type => (Action<IEvent, TableRow>)GetType().GetMethod("MakeEventVerifierMethod", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(type).Invoke(this, new object[] { }));

            foreach (var row in table.Rows)
            {
                var type = _typeMapping[row["Type"]];
                var @event = factories[type](row);

                var (connection, message) = _converter.ToMessage(@event) ?? (null, null);
                message.Topic.Should().Be(row["ExpectedTopic"]);

                var convertedEvent = _converter.ToEvent(connection, message);
                verifiers[type](convertedEvent, row);
            }
        }

        private Func<TableRow, IEvent> MakeInstanceFactoryMethod<T>() where T : class, IEvent
        {
            var factory = _container.Resolve<IEventInstanceFactory<T>>();

            return (row) => factory.FromTableRow(row);
        }

        private Action<IEvent, TableRow> MakeEventVerifierMethod<T>() where T : class, IEvent
        {
            var verifier = _container.Resolve<IProducedEventVerifier<T>>();

            return (@event, row) => verifier.VerifyFromTableRow(@event as T, row);
        }
    }
}
