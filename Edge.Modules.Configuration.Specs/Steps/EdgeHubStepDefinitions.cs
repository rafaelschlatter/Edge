using Autofac;
using TechTalk.SpecFlow;
using System;

namespace Edge.Modules.Configuration.Specs.Steps
{
    [Binding]
    public sealed class EdgeHubStepDefinitions
    {

        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly ScenarioContext _scenarioContext;
        private ContainerBuilder _builder;
        private IContainer _container;


        [Given("an Autofac context")]
        public void GivenAnAutofacContext()
        {
            _builder = new ContainerBuilder();
        }

        [Given("module (.*) from (.*) is registered")]
        public void GivenNamedModuleIsRegistered(string moduleName, string assemblyName)
        {
            var moduleType = Type.GetType($"{moduleName}, {assemblyName}");
            var module = (Autofac.Core.IModule) Activator.CreateInstance(moduleType);
            _builder.RegisterModule(module);
        }


        [Given("application container has been built")]
        public void GivenApplicationContainerHasBeenBuilt()
        {
            _container = _builder.Build();
        }



        [When("Building application container")]
        public void WhenBuildingContainer()
        {
            _container = _builder.Build();
        }

        [Then("Starting lifetime scope should succeed")]
        public void ThenBuildingScopeShouldSucceed()
        {
            using var scope = _container.BeginLifetimeScope();
        }

    }


}
