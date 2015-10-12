using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncResiliencyPatterns
{
    internal class CircuitBreakerStateParameters
    {
        public CircuitBreakerStateMachine stateMachine { get; set; }
        public CircuitBreakerSettings settings { get; set; }
        public ResiliencyCommandInvoker invoker { get; set; }        
    }
}
