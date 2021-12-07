using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using RaaLabs.Edge.Modules.Mqtt;
using FluentAssertions;

namespace RaaLabs.Edge.Modules.Mqtt.Specs.Steps
{
    [Binding]
    public sealed class TemplatedStringSteps
    {
        private TemplatedString<StringTemplateProperties> _templatedString;

        public TemplatedStringSteps()
        {
        }

        [Given(@"the templated string ""(.*)""")]
        public void GivenTheFollowingTemplatedString(string template)
        {
            _templatedString = new TemplatedString<StringTemplateProperties>(template);
        }

        [Then(@"we should expect the following mapping from input string to mapped variable")]
        public void ThenWeShouldExpectTheFollowingMappingFromInputToOutput(Table table)
        {
            foreach (var row in table.Rows)
            {
                var input = row["Input"];
                var output = new StringTemplateProperties();

                _templatedString.ExtractTo(input, output);

                output.SomeField.Should().Be(row["SomeField"]);
                output.OtherField.Should().Be(row["OtherField"]);
            }
        }

        [Then(@"we should expect the following mapping from input variable to output string")]
        public void ThenWeShouldExpectTheFollowingMappingFromInputVariableToOutputString(Table table)
        {
            foreach (var row in table.Rows)
            {
                var input = new StringTemplateProperties
                {
                    SomeField = row["SomeField"],
                    OtherField = row["OtherField"],
                };

                var actualOutput = _templatedString.BuildFrom(input);

                actualOutput.Should().Be(row["Output"]);
            }
        }

        private class StringTemplateProperties
        {
            public string SomeField { get; set; }
            public string OtherField { get; set; }
        }
    }
}
