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

namespace RaaLabs.Edge.Modules.Mqtt.Specs.Steps
{
    [Binding]
    class MqttTopicMapperSteps
    {
        private TypeMapping _typeMapping;
        private MqttTopicMapper _topicMapper;

        public MqttTopicMapperSteps(TypeMapping typeMapping)
        {
            _typeMapping = typeMapping;
        }

        [Given(@"an MqttTopicMapper with the following event types")]
        public void GivenAnMqttRouterWithTheFollowingRoutes(Table table)
        {
            var eventTypes = table.Rows
                .Select(row => _typeMapping[row["Type"]])
                .ToList();
            var inputTypes = eventTypes.Where(type => type.IsAssignableTo<IMqttIncomingEvent>()).ToHashSet();
            var outputTypes = eventTypes.Where(type => type.IsAssignableTo<IMqttOutgoingEvent>()).ToHashSet();

            var incomingEventHandlerMock = new Mock<IEventHandler<IMqttIncomingEvent>>();
            var outgoingEventHandlerMock = new Mock<IEventHandler<IMqttOutgoingEvent>>();
            incomingEventHandlerMock.Setup(handler => handler.GetSubtypes()).Returns(inputTypes);
            outgoingEventHandlerMock.Setup(handler => handler.GetSubtypes()).Returns(outputTypes);

            _topicMapper = new MqttTopicMapper(incomingEventHandlerMock.Object, outgoingEventHandlerMock.Object);


        }

        [Then(@"the MqttTopicMapper will give the following output")]
        public void ThenTheMqttTopicMapperWillGiveTheFollowingOutput(Table table)
        {
            foreach (var row in table.Rows)
            {
                var mappedType = _topicMapper.Resolve(_typeMapping[row["Connection"]], row["Topic"]);
                mappedType.Should().Be(_typeMapping[row["OutputType"]]);
            }

        }
    }
}
