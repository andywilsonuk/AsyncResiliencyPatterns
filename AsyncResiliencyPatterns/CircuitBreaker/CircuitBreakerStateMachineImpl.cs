using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncResiliencyPatterns
{
    internal sealed class CircuitBreakerStateMachineImpl : CircuitBreakerStateMachine, IDisposable
    {
        private CircuitBreakerStateInternal currentState;

        public CircuitBreakerStateMachineImpl(CircuitBreakerStateInternal initialState)
        {
            this.currentState = initialState;
        }

        public event EventHandler<CircuitBreakerStateChangedEventArgs> StateChanged;

        public CircuitBreakerStateInternal State
        {
            get { return this.currentState; }
            set { this.Transitioning(value); }
        }

        private void Transitioning(CircuitBreakerStateInternal newState)
        {
            if (newState.GetType() == this.currentState.GetType()) return;
            var previous = this.State;
            var args = new CircuitBreakerStateChangedEventArgs
            {
                Previous = previous,
                Current = newState
            };

            this.currentState = newState;
            this.OnStateChanged(args);
            this.DisposeState(previous);
        }

        private void OnStateChanged(CircuitBreakerStateChangedEventArgs args)
        {
            if (this.StateChanged != null) this.StateChanged(this, args);
        }

        private void DisposeState(CircuitBreakerStateInternal existingState)
        {
            IDisposable disposable = existingState as IDisposable;
            if (disposable == null) return;

            disposable.Dispose();
        }

        public void Dispose()
        {
            this.DisposeState(this.State);
        }
    }
}
