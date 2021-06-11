using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace RaaLabs.Edge
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBootloader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void RunBootloader(ContainerBuilder builder);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Status Status { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// 
        /// </summary>
        Waiting,

        /// <summary>
        /// 
        /// </summary>
        Ready,

        /// <summary>
        /// 
        /// </summary>
        Complete
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDependOn<T>
        where T : IBootloader
    {
    }
}
