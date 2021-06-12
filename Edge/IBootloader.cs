using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace RaaLabs.Edge
{
    /// <summary>
    /// Represents a class that should run before the application starts.
    /// 
    /// Usually, a bootloading class will implement one or more of the following interfaces, each
    /// defining functions to be run at different stages of the boot process for the current scope:
    /// - <see cref="IPreRegistrationStage"/>
    /// - <see cref="IRegistrationStage"/>
    /// - <see cref="IPostRegistrationStage"/>
    /// 
    /// The Status property tells the bootloading process what the status for the bootloader is.
    /// </summary>
    public interface IBootloader
    {
        /// <summary>
        /// The bootloader status.
        /// </summary>
        /// <returns></returns>
        public Status Status { get; }
    }

    /// <summary>
    /// A bootloader that needs to do something to the previous scope. This can be resolving components, etc.
    /// </summary>
    public interface IPreRegistrationStage : IBootloader
    {
        /// <summary>
        /// Function that will be run before a new scope is built.
        /// </summary>
        /// <param name="oldScope"></param>
        public void PreRegistration(ILifetimeScope oldScope);
    }

    /// <summary>
    /// A bootloader that needs to modify the <see cref="ContainerBuilder"/> in some way. This is usually registering new components to the application.
    /// </summary>
    public interface IRegistrationStage : IBootloader
    {
        /// <summary>
        /// Function that will be run to build a new scope
        /// </summary>
        /// <param name="builder"></param>
        public void RegistrationStage(ContainerBuilder builder);
    }

    /// <summary>
    /// A bootloader that needs to do something to the current scope. This can be instantiating components, etc.
    /// </summary>
    public interface IPostRegistrationStage : IBootloader
    {
        /// <summary>
        /// Function that will be run after the scope has been built.
        /// </summary>
        /// <param name="newScope"></param>
        public void PostRegistration(ILifetimeScope newScope);
    }

    /// <summary>
    /// The bootloader status.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// The bootloader is waiting for another bootloader to finish.
        /// </summary>
        Waiting,

        /// <summary>
        /// The bootloader is ready to run.
        /// </summary>
        Ready,

        /// <summary>
        /// The bootloader has finished running.
        /// </summary>
        Complete
    }
}
