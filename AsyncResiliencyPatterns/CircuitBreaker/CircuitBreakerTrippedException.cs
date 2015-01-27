using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AsyncResiliencyPatterns
{
    /// <summary>
    /// Occurs when the Circuit Breaker is in the tripped state.
    /// </summary>
    [Serializable]
    public class CircuitBreakerTrippedException : Exception
    {
        public CircuitBreakerTrippedException()
            : base("The circuit breaker has been tripped and no more requests will be processed until reset.")
        {
        }

        public CircuitBreakerTrippedException(string message)
            : base(message)
        {
        }

        public CircuitBreakerTrippedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected CircuitBreakerTrippedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
