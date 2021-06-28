using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Serilog;
using System.Runtime.CompilerServices;

namespace RaaLabs.Edge
{
    /// <summary>
    /// 
    /// </summary>
    public static class TaskHelpers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="tasks"></param>
        /// <param name="fileName"></param>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        public static async Task WhenAllWithLoggedExceptions(ILogger logger, IEnumerable<Task> tasks, [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            var tasksSet = tasks.ToHashSet();
            while (tasksSet.Count > 0)
            {
                var endedTask = await Task.WhenAny(tasksSet);
                if (endedTask.IsFaulted)
                {
                    logger.Error("Subtask called in '{OriginFile}', line {OriginLine} threw an exception:'", fileName, lineNumber);
                    foreach (var exception in endedTask.Exception?.InnerExceptions)
                    {
                        logger.Error(" - ({MessageType}) : '{Message}'", exception.GetType().FullName, exception.Message);
                    }
                }
                tasksSet.Remove(endedTask);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static async Task WhenAllRethrowExceptions(params Task[] tasks)
        {
            await WhenAllRethrowExceptions(tasks.AsEnumerable());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static async Task WhenAllRethrowExceptions(IEnumerable<Task> tasks)
        {
            var tasksSet = tasks.ToHashSet();
            while (tasksSet.Count > 0)
            {
                var endedTask = await Task.WhenAny(tasksSet);
                if (endedTask.IsFaulted)
                {
                    throw endedTask.Exception;
                }
            }
        }
    }
}
