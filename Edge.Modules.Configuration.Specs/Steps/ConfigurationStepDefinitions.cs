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
        private MockFileSystem _fs;
        private Moq.Mock<IApplicationShutdownTrigger> _restartTrigger;

        public ConfigurationStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given("an ApplicationBuilder")]
        public void GivenAnApplicationBuilder()
        {
            var builder = new ApplicationBuilder();
            builder.WithTask<DummyTask>();
            _scenarioContext.Add("builder", builder);
        }

        [Given("a mock filesystem containing configuration file in the (.*) directory")]
        public void GivenAMockFileSystem(string directory)
        {
            var builder = (ApplicationBuilder)_scenarioContext["builder"];
            _fs = new MockFileSystem();
            var confFile = new MockFileData(@"
            {
                ""someField"": ""yes""
            }
            ");
            _fs.AddFile($"{directory}/myconfiguration.json", confFile);
            builder.WithManualRegistration(b => b.RegisterInstance(_fs).As<IFileSystem>());
        }

        [Given("Configuration module is registered")]
        public void GivenConfigurationModuleIsRegistered()
        {
            _restartTrigger = new Moq.Mock<IApplicationShutdownTrigger>();
            var builder = (ApplicationBuilder)_scenarioContext["builder"];
            builder.WithModule<Configuration>();
            builder.WithManualRegistration(_ => _.RegisterInstance(_restartTrigger.Object).As<IApplicationShutdownTrigger>());
        }

        [Given("application has been built")]
        public void GivenApplicationHasBeenBuilt()
        {
            var builder = (ApplicationBuilder)_scenarioContext["builder"];
            var application = builder.Build();
            _scenarioContext.Add("application", application);
        }

        [Given("application has been running for one second")]
        public void GivenApplicationHasBeenRunningForOneSecond()
        {
            var application = (Application)_scenarioContext["application"];
            var runningTask = application.Run();
            _scenarioContext.Add("runningTask", runningTask);
            Task.Delay(1000).Wait();
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

        [When("configuration file is changed")]
        public void WhenChangingConfigurationFile()
        {
            _fs.File.SetLastWriteTimeUtc("data/myconfiguration.json", DateTime.UtcNow);
        }

        [Then("application restart will be triggered within two seconds")]
        public void ThenApplicationRestartWillBeTriggered()
        {
            Task.Delay(2000).Wait();
            _restartTrigger.Verify(_ => _.ShutdownApplication(), Moq.Times.AtLeastOnce);
        }

        [Then("the configuration object should contain correct configuration data")]
        public void ThenTheConfigurationObjectShouldContainCorrectConfigurationData()
        {
            var conf = (MyConfiguration)_scenarioContext["conf"];
            conf.SomeField.Should().Be("yes");
        }

        [RestartOnChange]
        [Name("myconfiguration.json")]
        class MyConfiguration : IConfiguration
        {
            public string SomeField { get; set; }
        }

        /// <summary>
        /// Task to ensure that the configuration object is instantiated
        /// </summary>
        class DummyTask : IRunAsync
        {
            public DummyTask(MyConfiguration conf)
            {

            }

            public async Task Run()
            {
                await Task.CompletedTask;
            }
        }
    }
}
