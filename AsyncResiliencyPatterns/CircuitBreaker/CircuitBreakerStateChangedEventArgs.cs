using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncResiliencyPatterns
{
    /// <summary>
    /// Occurs when a Circuit Breaker state transition takes place.
    /// </summary>
    public class CircuitBreakerStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Get the state being transitioned from.
        /// </summary>
        public CircuitBreakerState Previous { get; internal set; }

        /// <summary>
        /// Gets the state being transitioned to.
        /// </summary>
        public CircuitBreakerState Current { get; internal set; }
    }
}
