using System;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EventHandling
{
    public interface IConsumeEvent { }

    public interface IConsumeEvent<T> : IConsumeEvent
    {
        public void Handle(T @event);

    }
}
