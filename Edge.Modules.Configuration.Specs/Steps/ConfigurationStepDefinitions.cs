using Autofac;
using TechTalk.SpecFlow;
using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.Configuration.Specs.Steps
{
    [Binding]
    public sealed class ConfigurationStepDefinitions
    {
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly ScenarioContext _scenarioContext;

        public ConfigurationStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given("an ApplicationBuilder")]
        public void GivenAnApplicationBuilder()
        {
            _scenarioContext.Add("builder", new ApplicationBuilder());
        }

        [Given("a mock filesystem containing configuration file")]
        public void GivenAMockFileSystem()
        {
            var builder = (ApplicationBuilder)_scenarioContext["builder"];
            var fs = new MockFileSystem();
            var confFile = new MockFileData(@"
            {
                ""someField"": ""yes""
            }
            ");
            fs.AddFile("data/myconfiguration.json", confFile);
            builder.WithManualRegistration(b => b.RegisterInstance(fs).As<IFileSystem>());
        }

        [Given("Configuration module is registered")]
        public void GivenConfigurationModuleIsRegistered()
        {
            var builder = (ApplicationBuilder)_scenarioContext["builder"];
            builder.WithModule<Configuration>();
        }

        [Given("application has been built")]
        public void GivenApplicationHasBeenBuilt()
        {
            var builder = (ApplicationBuilder)_scenarioContext["builder"];
            var application = builder.Build();
            _scenarioContext.Add("application", application);
        }

        [Then("Starting lifetime scope should succeed")]
        public void ThenBuildingScopeShouldSucceed()
        {
            var application = (Application)_scenarioContext["application"];
            var container = application.Container;
            using var scope = container.BeginLifetimeScope();
        }

        [When("Resolving configuration class")]
        public void ThenResolvingConfigurationClassShouldSucceed()
        {
            var application = (Application)_scenarioContext["application"];
            var container = application.Container;
            var scope = container.BeginLifetimeScope();
            _scenarioContext.Add("conf", scope.Resolve<MyConfiguration>());
        }

        [Then("the configuration object should contain correct configuration data")]
        public void ThenTheConfigurationObjectShouldContainCorrectConfigurationData()
        {
            var conf = (MyConfiguration)_scenarioContext["conf"];
            conf.SomeField.Should().Be("yes");
        }

        [Name("myconfiguration.json")]
        class MyConfiguration : IConfiguration
        {
            public string SomeField { get; set; }
        }
    }
}
