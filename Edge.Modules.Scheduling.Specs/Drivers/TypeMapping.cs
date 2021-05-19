using RaaLabs.Edge.Modules.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.Scheduling.Specs.Drivers
{
    public class TypeMapping : Dictionary<string, Type>
    {
        public TypeMapping()
        {
            Add("EventHandling", typeof(EventHandling.EventHandling));
            Add("Scheduling", typeof(Scheduling));
            Add("CronEventHandler", typeof(CronEventHandler));
        }
    }
}
