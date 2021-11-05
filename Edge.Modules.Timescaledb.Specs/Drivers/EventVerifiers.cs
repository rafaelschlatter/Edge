using TechTalk.SpecFlow;
using FluentAssertions;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json.Linq;

namespace RaaLabs.Edge.Modules.Timescaledb.Specs.Drivers
{


    class SomeTimescaledbEventVerifier : IProducedEventVerifier<SomeTimescaledbEvent>
    {
        public void VerifyFromTableRow(SomeTimescaledbEvent data, TableRow row)
        {
            var expectedValue = int.Parse(row["Value"]);
            data.Value.Should().Be(expectedValue);
        }
    }

    class ObjectEventVerifier : IProducedEventVerifier<object>
    {
        public void VerifyFromTableRow(object data, TableRow row)
        {
            if (data is SomeTimescaledbEvent someTimescaledbEvent)
            {
                var expectedValue = int.Parse(row["Value"]);
                someTimescaledbEvent.Value.Should().Be(expectedValue);
            }
            else if (data is AnotherTimescaledbEvent anotherTimescaledbEvent)
            {
                anotherTimescaledbEvent.Value.Should().Be(row["Value"]);
            }
            else if (data is ThirdTimescaledbEvent thirdTimescaledbEvent)
            {
                var expectedValue = float.Parse(row["Value"]);
                thirdTimescaledbEvent.Value.Should().BeApproximately(expectedValue, 0.001f);
            }
        }
    }
}
