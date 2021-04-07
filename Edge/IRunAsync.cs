using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge
{
    /// <summary>
    /// Interface for tasks which should start to run in parallel when application starts.
    /// </summary>
    public interface IRunAsync
    {

        /// <summary>
        /// This function will run on application start. Classes implementing this interface should make
        /// this function async: public async Task Run() { ... }
        /// </summary>
        /// <returns></returns>
        public Task Run();
    }
}
