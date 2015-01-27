using System;
using System.Threading.Tasks;
namespace AsyncResiliencyPatterns
{
    internal interface CircuitBreakerStateInternal : CircuitBreakerState
    {
        Task<T> ExecuteAsync<T>(Func<Task<T>> task);
        void TransitionToAttempt();
        void TransitionToNormal();
        void TransitionToTripped();
    }
}
