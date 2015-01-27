using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncResiliencyPatterns
{
    internal class CircuitBreakerStateParameters
    {
        public CircuitBreakerStateParameters(CircuitBreakerStateMachine stateMachine, CircuitBreakerSettings settings, ResiliencyCommandInvoker invoker)
	    {
            this.stateMachine = stateMachine;
            this.settings = settings;
            this.invoker = invoker;
	    }

        public CircuitBreakerStateMachine stateMachine { get; private set; }
        public CircuitBreakerSettings settings { get; private set; }
        public ResiliencyCommandInvoker invoker { get; private set; }        
    }
}
