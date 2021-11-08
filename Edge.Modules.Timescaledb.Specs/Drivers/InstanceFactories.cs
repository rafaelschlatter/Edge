using System.Text;
using Microsoft.Azure.Devices.Client;
using TechTalk.SpecFlow;

namespace RaaLabs.Edge.Modules.Timescaledb.Specs.Drivers
{
    class SomeTimescaledbEventInstanceFactory : IEventInstanceFactory<SomeTimescaledbEvent>
    {
        public SomeTimescaledbEvent FromTableRow(TableRow row)
        {
            int value = int.Parse(row["Value"]);
            return new SomeTimescaledbEvent
            {
                Value = value
            };
        }
    }

    class AnotherTimescaledbEventInstanceFactory : IEventInstanceFactory<AnotherTimescaledbEvent>
    {
        public AnotherTimescaledbEvent FromTableRow(TableRow row)
        {
            return new AnotherTimescaledbEvent
            {
                Value = row["Value"]
            };
        }
    }

    class ThirdTimescaledbEventInstanceFactory : IEventInstanceFactory<ThirdTimescaledbEvent>
    {
        public ThirdTimescaledbEvent FromTableRow(TableRow row)
        {
            float value = float.Parse(row["Value"]);
            return new ThirdTimescaledbEvent
            {
                Value = value
            };
        }
    }
}
