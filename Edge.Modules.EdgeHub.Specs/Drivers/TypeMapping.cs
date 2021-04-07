using RaaLabs.Edge.Modules.EventHandling.Specs.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EdgeHub.Specs.Drivers
{
    public class TypeMapping : Dictionary<string, Type>
    {
        public TypeMapping()
        {
            Add("EventHandling", typeof(EventHandling.EventHandling));
            Add("EdgeHub", typeof(EdgeHub));
            Add("IntegerInputHandler", typeof(IntegerInputHandler));
            Add("IntegerInputSquaringHandler", typeof(IntegerInputSquaringHandler));
            Add("IntegerOutputHandler", typeof(IntegerOutputHandler));
        }
    }
}
