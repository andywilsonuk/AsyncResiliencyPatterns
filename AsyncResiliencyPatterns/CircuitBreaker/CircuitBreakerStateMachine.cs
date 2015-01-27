using System;
namespace AsyncResiliencyPatterns
{
    internal interface CircuitBreakerStateMachine : IDisposable
    {
        CircuitBreakerStateInternal State { get; set; }
        event EventHandler<CircuitBreakerStateChangedEventArgs> StateChanged;
    }
}
