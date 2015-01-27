using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncResiliencyPatterns
{
    /// <summary>
    /// Provides limited concurrent access to an async dependency.
    /// </summary>
    /// <remarks>Useful to prevent overloading a normally-running service.</remarks>
    public class Throttle : ResiliencyCommandInvoker
    {
        private SemaphoreSuperSlim semaphore;
        private ResiliencyCommandInvoker invoker;

        /// <summary>
        /// Initialises a new Throttle class.
        /// </summary>
        /// <param name="limit">The maximum concurrent executions.</param>
        public Throttle(int limit)
            : this(limit, new InnerCommandInvoker())
        {
        }

        /// <summary>
        /// Initialises a new Throttle class with a nested resiliency pattern invoker.
        /// </summary>
        /// <param name="limit">The maximum concurrent executions.</param>
        /// <param name="nestedInvoker">The invoker to pass the command onto.</param>
        public Throttle(int limit, ResiliencyCommandInvoker nestedInvoker)
        {
            if (limit < 1) throw new ArgumentOutOfRangeException("limit", "The limit cannot be less than one.");

            this.semaphore = new SemaphoreSuperSlim(limit);
            this.invoker = nestedInvoker;
        }

        /// <summary>
        /// Gets the number of instances available.
        /// </summary>
        public int InstancesRemaining
        {
            get { return this.semaphore.AvailableCount; }
        }

        /// <summary>
        /// Occurs when an execution is throttled.
        /// </summary>
        public event EventHandler Throttled;

        /// <summary>
        /// Executes the specified function within the bounds if the resiliency invoker and returns the value.
        /// </summary>
        /// <typeparam name="T">The type of return value.</typeparam>
        /// <param name="command">The function to execute.</param>
        /// <returns>The async Task with return type T.</returns>
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> command)
        {
            if (this.IsSaturated())
            {
                this.OnThrottled();
                throw new ThrottleLimitException();
            }

            try
            {
                return await this.invoker.ExecuteAsync(command);
            }
            finally
            {
                this.Release();
            }
        }

        /// <summary>
        /// Executes the specified action within the bounds if the resiliency invoker.
        /// </summary>
        /// <param name="command">The action to execute.</param>
        /// <returns>The async Task.</returns>
        public async Task ExecuteAsync(Func<Task> command)
        {
            if (this.IsSaturated())
            {
                this.OnThrottled();
                throw new ThrottleLimitException();
            }

            try
            {
                await this.invoker.ExecuteAsync(command);
            }
            finally
            {
                this.Release();
            }
        }

        private bool IsSaturated()
        {
            return !this.semaphore.Acquire();
        }

        private void OnThrottled()
        {
            if (this.Throttled != null) this.Throttled(this, new EventArgs());
        }

        private void Release()
        {
            this.semaphore.Release();
        }
    }
}
