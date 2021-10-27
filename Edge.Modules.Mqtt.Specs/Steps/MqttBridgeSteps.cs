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

namespace RaaLabs.Edge.Modules.Mqtt.Specs.Steps
{
    [Binding]
    class MqttBridgeSteps
    {
        private readonly IObjectContainer _container;

        public MqttBridgeSteps(IObjectContainer container)
        {
            _container = container;
        }

        [BeforeScenario]
        public void SetupEventFactories()
        {
            _container.RegisterTypeAs<SomeMqttOutgoingEventInstanceFactory, IEventInstanceFactory<SomeMqttOutgoingEvent>>();
            _container.RegisterTypeAs<AnotherMqttOutgoingEventInstanceFactory, IEventInstanceFactory<AnotherMqttOutgoingEvent>>();
            _container.RegisterTypeAs<TopicInstanceFactory, IEventInstanceFactory<string>>();
            _container.RegisterTypeAs<MqttApplicationMessageInstanceFactory, IEventInstanceFactory<MqttApplicationMessage>>();

            _container.RegisterTypeAs<MqttApplicationMessageVerifier, IProducedEventVerifier<MqttApplicationMessage>>();
            _container.RegisterTypeAs<SomeMqttIncomingEventVerifier, IProducedEventVerifier<SomeMqttIncomingEvent>>();
            _container.RegisterTypeAs<AnotherMqttIncomingEventVerifier, IProducedEventVerifier<AnotherMqttIncomingEvent>>();
        }
    }
}
