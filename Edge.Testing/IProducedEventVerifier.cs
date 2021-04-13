using TechTalk.SpecFlow;

namespace RaaLabs.Edge
{
    /// <summary>
    /// Interface for a class able to verify an event of type T from a SpecFlow table row.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IProducedEventVerifier<T>
    {
        /// <summary>
        /// Verify an event from a SpecFlow table row
        /// </summary>
        /// <param name="event">The event to verify</param>
        /// <param name="row">The SpecFlow table row</param>
        public void VerifyFromTableRow(T @event, TableRow row);
    }
}
