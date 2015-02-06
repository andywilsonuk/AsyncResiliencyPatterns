using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncResiliencyPatterns
{
    /// <summary>
    /// Represents a circuit that has recently failure but is in a retry state.
    /// </summary>
    public class CircuitBreakerStateAttempt : CircuitBreakerStateInternal, CircuitBreakerState, CircuitBreakerShortCircuitableState
    {
        private CircuitBreakerStateParameters parameters;
        private SemaphoreSuperSlim semaphore;

        internal CircuitBreakerStateAttempt(CircuitBreakerStateParameters parameters)
        {
            this.parameters = parameters;
            this.semaphore = new SemaphoreSuperSlim(1);
        }

        /// <summary>
        /// Occurs when the tripped circuit forces a short (i.e. immediately fails the command).
        /// </summary>
        public event EventHandler ShortCircuited;

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> task)
        {
            if (!this.semaphore.Acquire())
            {
                this.OnShortCircuited();
                throw new CircuitBreakerTrippedException();
            }

            T result;
            try
            {
                result = await this.parameters.invoker.ExecuteAsync(task);
                this.TransitionToNormal();
            }
            catch (Exception ex)
            {
                if (this.parameters.settings.ShouldFailureBeRecorded(ex)) this.TransitionToTripped();
                throw;
            }

            return result;
        }

        private void OnShortCircuited()
        {
            if (this.ShortCircuited != null) this.ShortCircuited(this, new EventArgs());
        }

        public void TransitionToAttempt()
        {
        }

        public void TransitionToNormal()
        {
            this.parameters.stateMachine.State = new CircuitBreakerNormalState(this.parameters);
        }

        public void TransitionToTripped()
        {
            this.parameters.stateMachine.State = new CircuitBreakerStateTripped(this.parameters);
        }
    }
}
