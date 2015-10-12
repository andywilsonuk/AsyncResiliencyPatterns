using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace AsyncResiliencyPatterns.IntegrationTests
{
    [Binding]
    public class CircuitBreakerSteps
    {
        private CircuitBreaker circuitBreaker;

        [Given(@"I have a circuit breaker with settings")]
        public void GivenIHaveACircuitBreakerWithSettings(Table table)
        {
            CircuitBreakerSettings settings = table.CreateInstance<CircuitBreakerSettings>();
            this.circuitBreaker = new CircuitBreaker(settings);
        }

        [When(@"I execute the failing IO method through the Circuit Breaker")]
        public void WhenIExecuteTheFailingIOMethodThroughTheCircuitBreaker()
        {
            Func<Task> func = () =>
            {
                throw new IOException();
            };

            try
            {
                this.circuitBreaker.ExecuteAsync(func).Wait();
            }
            catch(AggregateException ex)
            {
                if (!(ex.InnerException is IOException)) throw;
            }
        }

        [Then(@"the Circuit Breaker state should be Tripped")]
        public void ThenTheCircuitBreakerStateShouldBeTripped()
        {
            Assert.IsInstanceOfType(this.circuitBreaker.State, typeof(CircuitBreakerStateTripped));
        }
    }
}
