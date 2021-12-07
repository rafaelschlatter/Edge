using Autofac;
using TechTalk.SpecFlow;
using FluentAssertions;
using RaaLabs.Edge.Modules.Scheduling.Specs.Drivers;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace RaaLabs.Edge.Modules.Scheduling.Specs.Steps
{
    [Binding]
    public sealed class SchedulingSteps
    {
        private readonly ApplicationContext _appContext;
        private readonly TypeMapping _typeMapping;
        private Task _applicationTask;

        private SemaphoreSlim _applicationHasRunForTwoSeconds;

        public SchedulingSteps(ApplicationContext appContext, TypeMapping typeMapping)
        {
            _appContext = appContext;
            _typeMapping = typeMapping;
            _applicationHasRunForTwoSeconds = new SemaphoreSlim(0, 1);
        }

        [When(@"the application has been running for (.*) milliseconds")]
        public async Task WhenTheApplicationHasBeenRunningForNMilliseconds(int milliseconds)
        {
            var application = _appContext.Application;
            _applicationTask = application.Run();
            await Task.WhenAny(_applicationTask, Task.Delay(milliseconds));
            _applicationHasRunForTwoSeconds.Release();
        }

        [Then(@"there should be at least this many events produced")]
        public async Task ThenThereShouldBeAtLeastThisManyEventsProduced(Table table)
        {
            await _applicationHasRunForTwoSeconds.WaitAsync();
            _applicationHasRunForTwoSeconds.Release();

            var eventCounter = _appContext.Application.RuntimeScope.Resolve<EventCounter>();
            foreach (var row in table.Rows)
            {
                var eventType = _typeMapping[row["EventType"]];
                var eventsProduced = eventCounter.GetEventsProducedForType(eventType);
                var numEventsProduced = eventsProduced?.Count ?? 0;
                numEventsProduced.Should().BeGreaterThan(int.Parse(row["Count"]));
            }
        }

        [Then(@"events with payload should contain the correct payload")]
        public async Task ThenEventsWithPayloadShouldContainTheCorrectPayload()
        {
            await _applicationHasRunForTwoSeconds.WaitAsync();
            _applicationHasRunForTwoSeconds.Release();

            var eventCounter = _appContext.Application.RuntimeScope.Resolve<EventCounter>();
            var eventsProduced = eventCounter.GetEventsProducedForType(typeof(TypeScheduledEvent))
                .Select(_ => _ as TypeScheduledEvent)
                .ToList();

            var tagsProduced = eventsProduced
                .SelectMany(e => e.Tags)
                .GroupBy(tag => tag)
                .ToDictionary(uniqueTag => uniqueTag.Key, uniqueTag => uniqueTag.Count());

            // events containing "tag-3" and "tag-4" should be produced 4 times as often as the events
            // containing "tag-1" and "tag-2".
            var fraction = tagsProduced["tag-3"] / (double)tagsProduced["tag-1"];
            fraction.Should().BeGreaterThan(2.0);
        }

    }
}
