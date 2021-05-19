using Autofac;
using Autofac.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using FluentAssertions;
using RaaLabs.Edge.Modules.Scheduling.Specs.Drivers;

namespace RaaLabs.Edge.Modules.Scheduling.Specs.Steps
{
    [Binding]
    public sealed class SchedulingSteps
    {
        private readonly ApplicationContext _appContext;
        private readonly TypeMapping _typeMapping;

        public SchedulingSteps(ApplicationContext appContext, TypeMapping typeMapping)
        {
            _appContext = appContext;
            _typeMapping = typeMapping;
        }

    }
}
