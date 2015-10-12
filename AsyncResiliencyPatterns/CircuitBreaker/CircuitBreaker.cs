using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncResiliencyPatterns
{
    /// <summary>
    /// Provides automatic failure cut-off for async dependencies so that they are given time to recover.
    /// </summary>
    /// <remarks>Useful to prevent overloading a failing service.</remarks>
    public sealed class CircuitBreaker : ResiliencyCommandInvoker, IDisposable
    {
        private CircuitBreakerStateMachine stateMachine;

        /// <summary>
        /// Initialises a new circuit breaker using the supplied settings.
        /// </summary>
        /// <param name="settings">Settings to configure the circuit breaker.</param>
        public CircuitBreaker(CircuitBreakerSettings settings)
            : this(settings, new InnerCommandInvoker())
        {
        }

        /// <summary>
        /// Initialises a new circuit breaker using the supplied settings with a nested resiliency pattern invoker.
        /// </summary>
        /// <param name="settings">Settings to configure the circuit breaker.</param>
        /// <param name="nestedInvoker">The invoker to pass the command onto.</param>
        public CircuitBreaker(CircuitBreakerSettings settings, ResiliencyCommandInvoker nestedInvoker)
        {
            if (settings == null) settings = new CircuitBreakerSettings();
            var parameters = new CircuitBreakerStateParameters
            {
                settings = settings,
                invoker = nestedInvoker
            };
            this.stateMachine = new CircuitBreakerStateMachineImpl(new CircuitBreakerNormalState(parameters));
            parameters.stateMachine = this.stateMachine;
            this.AttachToStateChangeEvent();
        }

        internal CircuitBreaker(CircuitBreakerStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
            this.AttachToStateChangeEvent();
        }

        /// <summary>
        /// Represents the method that will handle an event when the event provides CircuitBreakerStateChangedEventArgs data.
        /// </summary>
        public event EventHandler<CircuitBreakerStateChangedEventArgs> StateChanged;

        /// <summary>
        /// Gets the current state of the circuit.
        /// </summary>
        public CircuitBreakerState State
        {
            get { return this.stateMachine.State; }
        }

        /// <summary>
        /// Executes the specified action within the bounds if the resiliency invoker.
        /// </summary>
        /// <param name="command">The action to execute.</param>
        /// <returns>The async Task.</returns>
        public async Task ExecuteAsync(Func<Task> command)
        {
            Func<Task<bool>> func = async () =>
            {
                await command();
                return true;
            };

            await this.ExecuteAsync<bool>(func);
        }

        /// <summary>
        /// Executes the specified function within the bounds if the resiliency invoker and returns the value.
        /// </summary>
        /// <typeparam name="T">The type of return value.</typeparam>
        /// <param name="command">The function to execute.</param>
        /// <returns>The async Task with return type T.</returns>
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> command)
        {
            return await this.stateMachine.State.ExecuteAsync(command);
        }

        private void AttachToStateChangeEvent()
        {
            this.stateMachine.StateChanged += this.OnStateChanged;
        }

        private void OnStateChanged(object sender, CircuitBreakerStateChangedEventArgs args)
        {
            if (this.StateChanged != null) this.StateChanged(this, args);
        }

        public void Dispose()
        {
            this.stateMachine.Dispose();
        }
    }
}
