using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncResiliencyPatterns
{
    /// <summary>
    /// Represents a circuit that is unavailable (tripped).
    /// </summary>
    public sealed class CircuitBreakerStateTripped : CircuitBreakerStateInternal, CircuitBreakerState, CircuitBreakerShortCircuitableState, IDisposable
    {
        private static readonly TimeSpan infinity = TimeSpan.FromMilliseconds(-1);
        private Timer resetTimer;
        private CircuitBreakerStateParameters parameters;

        internal CircuitBreakerStateTripped(CircuitBreakerStateParameters parameters)
        {
            this.parameters = parameters;
            this.resetTimer = new Timer(this.ResetCallback, null, this.parameters.settings.TripWaitPeriod, infinity);
        }

        /// <summary>
        /// Occurs when the tripped circuit forces a short (i.e. immediately fails the command).
        /// </summary>
        public event EventHandler ShortCircuited;

        public Task<T> ExecuteAsync<T>(Func<Task<T>> task)
        {
            this.OnShortCircuited();
            throw new CircuitBreakerTrippedException();
        }

        private void OnShortCircuited()
        {
            if (this.ShortCircuited != null) this.ShortCircuited(this, new EventArgs());
        }

        public void TransitionToAttempt()
        {
            this.parameters.stateMachine.State = new CircuitBreakerStateAttempt(this.parameters);
        }

        public void TransitionToNormal()
        {
        }

        public void TransitionToTripped()
        {
        }

        internal void ResetCallback(object stateInfo)
        {
            this.TransitionToAttempt();
        }

        public void Dispose()
        {
            if (this.resetTimer == null) return;
            this.resetTimer.Dispose();
            this.resetTimer = null;
        }
    }
}
