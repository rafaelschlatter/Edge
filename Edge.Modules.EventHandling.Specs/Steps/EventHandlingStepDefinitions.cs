using Autofac;
using TechTalk.SpecFlow;
using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using System.Threading.Tasks;
using RaaLabs.Edge.Modules.EventHandling.Specs.Drivers;

namespace Edge.Modules.EventHandling.Specs.Steps
{
    [Binding]
    public sealed class EventHandlingStepDefinitions
    {


        private readonly ApplicationContext _appContext;
        private readonly TypeMapping _typeMapping;

        public EventHandlingStepDefinitions(ApplicationContext appContext, TypeMapping typeMapping)
        {
            _appContext = appContext;
            _typeMapping = typeMapping;
        }

        [When(@"event producer (.*) produces the following values")]
        public async Task WhenEventProducerProducesTheFollowingValues(string producerName, Table table)
        {
            await Task.Delay(100);
            var producer = _appContext.Instances[producerName];
            if (producer.GetType().IsAssignableTo<IProducer>())
            {
                var syncProducer = (IProducer)producer;
                foreach (var row in table.Rows)
                {
                    int value = int.Parse(row["Value"]);
                    syncProducer.Produce(value);
                }
            }
            else
            {
                var asyncProducer = (IAsyncProducer)producer;
                foreach (var row in table.Rows)
                {
                    int value = int.Parse(row["Value"]);
                    await asyncProducer.Produce(value);
                }
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
                var instance = (IConsumer)_appContext.Instances[handler];
                instance.ReceivedEvents.Should().BeEquivalentTo(expected);
            }
        }
    }
}
