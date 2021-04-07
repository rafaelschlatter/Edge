using Autofac;
using Autofac.Core;
using Newtonsoft.Json;
using RaaLabs.Edge.Modules.EdgeHub.Specs.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using FluentAssertions;
using RaaLabs.Edge.Modules.EventHandling.Specs.Drivers;

namespace RaaLabs.Edge.Modules.EdgeHub.Specs.Steps
{
    [Binding]
    public sealed class EdgeHubSteps
    {
        private readonly ApplicationContext _appContext;
        private readonly TypeMapping _typeMapping;

        public EdgeHubSteps(ApplicationContext appContext, TypeMapping typeMapping)
        {
            _appContext = appContext;
            _typeMapping = typeMapping;
        }

        [When(@"EdgeHub input (.*) receives the following values")]
        public void WhenEdgeHubInputReceivesTheFollowingValues(string inputName, Table table)
        {
            NullIotModuleClient client = (NullIotModuleClient)_appContext.Scope.Resolve<IIotModuleClient>();
            foreach (var row in table.Rows)
            {
                int value = int.Parse(row["Value"]);
                var incomingMessage = JsonConvert.SerializeObject(new EdgeHubIntegerInputEvent { Value = value });
                client.SimulateIncomingEvent(inputName, incomingMessage);
            }
        }

        [When(@"event producer (.*) produces the following values")]
        public void WhenEventProducerProducesTheFollowingValues(string producer, Table table)
        {
            var handler = (IntegerOutputHandler)_appContext.Instances["outputHandler"];
            foreach (var row in table.Rows)
            {
                int value = int.Parse(row["Value"]);
                handler.Send(value);
            }
        }

        [Then(@"handlers should receive the following values")]
        public void ThenHandlersShouldReceiveTheFollowingValues(Table table)
        {
            var handlers = table.Header.ToList();
            var expectedValuesForHandler = handlers.ToDictionary(_ => _, _ => new List<int>());

            foreach (var row in table.Rows)
            {
                foreach (var handler in handlers)
                {
                    expectedValuesForHandler[handler].Add(int.Parse(row[handler]));
                }
            }

            foreach (var (handler, expected) in expectedValuesForHandler)
            {
                var instance = (IntegerInputHandler)_appContext.Instances[handler];
                instance.Values.Should().BeEquivalentTo(expected);
            }
        }

        [Then(@"EdgeHub output (.*) sends the following values")]
        public void ThenEdgeHubOutputSendsTheFollowingValues(string outputName, Table table)
        {
            NullIotModuleClient client = (NullIotModuleClient)_appContext.Scope.Resolve<IIotModuleClient>();
            client.MessagesSent
                .Where(message => message.Item1 == outputName)
                .Select(message => message.Item2)
                .Select(_ => JsonConvert.DeserializeObject<EdgeHubIntegerOutputEvent>(_).Value).Should().BeEquivalentTo(table.Rows.Select(_ => int.Parse(_["Value"])));
        }
    }
}
