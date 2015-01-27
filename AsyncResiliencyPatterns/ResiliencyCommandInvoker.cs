using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncResiliencyPatterns
{
    /// <summary>
    /// Provides a common interface to allow Throttle and Circuit Breaker resiliency to be nested.
    /// </summary>
    /// <remarks>This is useful if the task is to be both throttled and also subject to a circuit breaker. 
    /// One preventing overloading a normally-running service and the other overloading a failing service.</remarks>
    public interface ResiliencyCommandInvoker
    {
        /// <summary>
        /// Executes the specified function within the bounds if the resiliency invoker and returns the value.
        /// </summary>
        /// <typeparam name="T">The type of return value.</typeparam>
        /// <param name="command">The function to execute.</param>
        /// <returns>The async Task with return type T.</returns>
        Task<T> ExecuteAsync<T>(Func<Task<T>> command);

        /// <summary>
        /// Executes the specified action within the bounds if the resiliency invoker.
        /// </summary>
        /// <param name="command">The action to execute.</param>
        /// <returns>The async Task.</returns>
        Task ExecuteAsync(Func<Task> command);
    }
}
