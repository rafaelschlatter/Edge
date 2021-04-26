using System.Collections.Generic;
using TechTalk.SpecFlow;

namespace RaaLabs.Edge
{
    /// <summary>
    /// Interface for a class able to verify all events of type T from a SpecFlow table.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAllProducedEventsVerifier<T>
    {
        /// <summary>
        /// Verify all events from a SpecFlow table
        /// </summary>
        /// <param name="events">The events to verify</param>
        /// <param name="table">The SpecFlow table</param>
        public void VerifyFromTable(IList<T> events, Table table);
    }
}
