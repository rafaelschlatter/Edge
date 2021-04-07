using RaaLabs.Edge;
using TechTalk.SpecFlow;
using FluentAssertions;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Specs.Steps
{
    [Binding]
    public sealed class ApplicationBuilderStepDefinitions
    {

        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly ScenarioContext _scenarioContext;

        public ApplicationBuilderStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given("an ApplicationBuilder")]
        public void AnApplicationBuilder()
        {
            _scenarioContext.Add("builder", new ApplicationBuilder());
        }

        [When("the application is built")]
        public void WhenTheApplicationIsBuilt()
        {
            var builder = (ApplicationBuilder) _scenarioContext["builder"];
            _scenarioContext.Add("application", builder.Build());
        }

        [Then("the application should be able to run")]
        public void ThenTheApplicationShouldBeAbleToRun()
        {
            var application = (Application)_scenarioContext["application"];
            Task.Run(async () =>
            {
                var applicationRunTask = application.Run();
                await Task.WhenAny(applicationRunTask, Task.Delay(1000));

                applicationRunTask.Exception.Should().BeNull();
            }).Wait();
        }
    }
}
