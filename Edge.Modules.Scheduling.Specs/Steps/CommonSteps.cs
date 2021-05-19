using Autofac.Core;
using RaaLabs.Edge.Modules.Scheduling.Specs.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        [Given(@"a lifetime scope with the following instances")]
        public void GivenTheFollowingInstances(Table table)
        {
            var mappings = table.Rows.Select(row => (Source: row["Source"], From: row["From"], To: row["To"]));

            var sources = mappings.GroupBy(_ => _.Source);

            var scope = _appContext.StartScope();
            foreach (var row in table.Rows)
            {
                var name = row["Name"];
                var typeName = row["Type"];
                var type = _typeMapping[typeName];

                var instance = _appContext.ResolveInstance(name, type);
            }
        }

        [Then(@"starting lifetime scope should succeed")]
        public void ThenBuildingScopeShouldSucceed()
        {
            var container = _appContext.Application.Container;
            using var scope = container.BeginLifetimeScope();
        }

    }
}
