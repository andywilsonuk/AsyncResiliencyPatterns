using System;
namespace AsyncResiliencyPatterns
{
    public interface CircuitBreakerShortCircuitableState
    {
        /// <summary>
        /// Occurs when the tripped circuit forces a short (i.e. immediately fails the command).
        /// </summary>
        event EventHandler ShortCircuited;
    }
}
