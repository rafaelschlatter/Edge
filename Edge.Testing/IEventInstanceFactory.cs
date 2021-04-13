using TechTalk.SpecFlow;

namespace RaaLabs.Edge
{
    /// <summary>
    /// Interface for a class able to construct an instance of type T from a SpecFlow table row
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEventInstanceFactory<T>
    {
        /// <summary>
        /// Function to create an instance of type T from a SpecFlow table row
        /// </summary>
        /// <param name="row">The SpecFlow table row</param>
        /// <returns>an instance of type T</returns>
        public T FromTableRow(TableRow row);
    }
}
