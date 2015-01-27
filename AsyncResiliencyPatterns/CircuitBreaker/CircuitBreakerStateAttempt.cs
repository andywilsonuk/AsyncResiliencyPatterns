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
    public class CircuitBreakerStateAttempt : CircuitBreakerStateInternal, CircuitBreakerState
    {
        private CircuitBreakerStateParameters parameters;

        internal CircuitBreakerStateAttempt(CircuitBreakerStateParameters parameters)
        {
            this.parameters = parameters;
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> task)
        {
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
