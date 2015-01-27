using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncResiliencyPatterns
{
    /// <summary>
    /// Represents a circuit that is functioning normally.
    /// </summary>
    public sealed class CircuitBreakerNormalState : CircuitBreakerStateInternal, CircuitBreakerState, IDisposable
    {
        private static readonly TimeSpan infinity = TimeSpan.FromMilliseconds(-1);
        private Timer resetTimer;
        private int failureCount = 0;
        private CircuitBreakerStateParameters parameters;

        internal CircuitBreakerNormalState(CircuitBreakerStateParameters parameters)
        {
            this.parameters = parameters;
            this.InitialiseResetTimer();
        }

        private void InitialiseResetTimer()
        {
            this.resetTimer = new Timer(this.ResetCallback, null, infinity, infinity);
        }

        /// <summary>
        /// Gets the number of commands that have failed within the reset period.
        /// </summary>
        public int FailureCount
        {
            get { return this.failureCount; }
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> task)
        {
            T result;
            try
            {
                result = await this.parameters.invoker.ExecuteAsync(task);
            }
            catch (Exception ex)
            {
                if (this.parameters.settings.ShouldFailureBeRecorded(ex)) this.RecordFailedAttempt();
                throw;
            }

            return result;
        }

        public void TransitionToAttempt()
        {
        }

        public void TransitionToNormal()
        {
        }

        public void TransitionToTripped()
        {
            this.parameters.stateMachine.State = new CircuitBreakerStateTripped(this.parameters);
        }

        private void RecordFailedAttempt()
        {
            this.StartExpiryCallbackTimer();

            Interlocked.Increment(ref this.failureCount);
            if (this.HasReachedThreshold) this.TransitionToTripped();
        }

        private void StartExpiryCallbackTimer()
        {
            var timer = this.resetTimer;
            if (timer == null) return;
            try
            {
                timer.Change(this.parameters.settings.FailureExpiryPeriod, infinity);
            }
            catch(ObjectDisposedException)
            { }
        }

        private bool HasReachedThreshold
        {
            get { return this.failureCount >= this.parameters.settings.FailureThreshold; }
        }

        internal void ResetCallback(object stateInfo)
        {
            Interlocked.Exchange(ref this.failureCount, 0);
        }

        public void Dispose()
        {
            if (this.resetTimer == null) return;
            var timer = this.resetTimer;
            this.resetTimer = null;
            timer.Dispose();
        }
    }
}
