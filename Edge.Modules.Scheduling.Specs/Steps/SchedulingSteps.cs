using Autofac;
using TechTalk.SpecFlow;
using FluentAssertions;
using RaaLabs.Edge.Modules.Scheduling.Specs.Drivers;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.Scheduling.Specs.Steps
{
    [Binding]
    public sealed class SchedulingSteps
    {
        private readonly ApplicationContext _appContext;
        private readonly TypeMapping _typeMapping;
        private Task _applicationTask;

        public SchedulingSteps(ApplicationContext appContext, TypeMapping typeMapping)
        {
            _appContext = appContext;
            _typeMapping = typeMapping;
        }

        [When(@"the application has been running for (.*) milliseconds")]
        public void WhenTheApplicationHasBeenRunningForNMilliseconds(int milliseconds)
        {
            var application = _appContext.Application;
            _applicationTask = application.Run();
            Task.WhenAny(_applicationTask, Task.Delay(milliseconds)).Wait();
        }

        [Then(@"there should be at least this many events produced")]
        public void ThenThereShouldBeAtLeastThisManyEventsProduced(Table table)
        {
            var eventCounter = _appContext.Application.Container.Resolve<EventCounter>();
            foreach (var row in table.Rows)
            {
                var eventType = _typeMapping[row["EventType"]];
                var eventsProduced = eventCounter.GetEventsProducedForType(eventType);
                eventsProduced.Should().BeGreaterThan(int.Parse(row["Count"]));
            }
        }



    }
}
