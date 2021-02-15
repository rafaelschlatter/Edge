using System;

namespace RaaLabs.Edge.Modules.Configuration
{
    public interface IConfiguration
    {
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NameAttribute : Attribute
    {
        public string Name { get; }
        public NameAttribute(string name)
        {
            Name = name;
        }
    }
}
