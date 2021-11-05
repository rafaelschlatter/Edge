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
    class MqttRouterSteps
    {
        MqttRouter<string> _router;

        public MqttRouterSteps()
        {
        }

        [Given(@"an MqttRouter with the following routes")]
        public void GivenAnMqttRouterWithTheFollowingRoutes(Table table)
        {
            var routes = table.Rows.Select(row => (pattern: row["Pattern"], target: row["Target"]));

            _router = new MqttRouter<string>(routes);
        }

        [Then(@"the MqttRouter will give the following output")]
        public void ThenTheMqttRouterWillGiveTheFollowingOutput(Table table)
        {
            foreach (var row in table.Rows)
            {
                _router.ResolvePath(row["Topic"]).Should().Be(row["Target"]);
            }
        }
    }
}
