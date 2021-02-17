using RaaLabs.Edge;
using TechTalk.SpecFlow;
using FluentAssertions;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace RaaLabs.Edge.Specs.Steps
{
    [Binding]
    public sealed class AsyncTasksStepDefinitions
    {

        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly ScenarioContext _scenarioContext;
        private static int _globalCounter;
        private static SemaphoreSlim _counterSemaphore;

        public AsyncTasksStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _globalCounter = 0;
            _counterSemaphore = new SemaphoreSlim(1);
        }

        [Given("three async tasks")]
        public void ThreeAsyncTasks()
        {
            var builder = (ApplicationBuilder)_scenarioContext["builder"];
            builder.WithTask<AsyncCounterTask>();
            builder.WithTask<AsyncCounterTask>();
            builder.WithTask<AsyncCounterTask>();
        }

        [When("the application is started")]
        public void GivenTheApplicationIsStarted()
        {
            var application = (Application)_scenarioContext["application"];
            Task.Run(async () =>
            {
                var applicationRunTask = application.Run();
                await Task.WhenAny(applicationRunTask, Task.Delay(1000));

            }).Wait();
        }
        
        [Then("the application should run all async tasks")]
        public void TheApplicationShouldRunALlAsyncTasks()
        {
            _globalCounter.Should().Be(45);
        }

        class AsyncCounterTask : IRunAsync
        {
            public async Task Run()
            {
                foreach (var _ in Enumerable.Range(0, 15))
                {
                    await _counterSemaphore.WaitAsync();
                    _globalCounter++;
                    _counterSemaphore.Release();
                    await Task.Yield();
                }
            }
        }
    }
}
