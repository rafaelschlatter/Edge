using RaaLabs.Edge.Modules.Scheduling.Specs.Drivers;
using TechTalk.SpecFlow;

namespace RaaLabs.Edge.Modules.Scheduling.Specs.Steps
{
    [Binding]
    public sealed class CommonSteps
    {
        private readonly ApplicationContext _appContext;
        private readonly TypeMapping _typeMapping;

        public CommonSteps(ApplicationContext appContext, TypeMapping typeMapping)
        {
            _appContext = appContext;
            _typeMapping = typeMapping;
        }

        [Given(@"an application with the following registrations")]
        public void GivenAnApplicationWithTheFollowingRegistrations(Table table)
        {
            foreach (var row in table.Rows)
            {
                var kindName = row["Kind"];
                var registrationTypeName = row["Type"];
                var registrationType = _typeMapping[registrationTypeName];
                var withRegistrationMethod = typeof(ApplicationContext).GetMethod($"With{kindName}").MakeGenericMethod(registrationType);
                withRegistrationMethod.Invoke(_appContext, new object[] { });
            }

            _appContext.Build();
        }

        [Then(@"starting lifetime scope should succeed")]
        public void ThenBuildingScopeShouldSucceed()
        {
            var container = _appContext.Application.Container;
            using var scope = container.BeginLifetimeScope();
        }
    }
}
