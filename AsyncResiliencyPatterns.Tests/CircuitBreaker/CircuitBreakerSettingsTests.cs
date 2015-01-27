using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncResiliencyPatterns.Tests
{
    [TestClass]
    public class CircuitBreakerSettingsTests
    {
        [TestMethod]
        public void ValidConstructor()
        {
            var options = new CircuitBreakerSettings();

            Assert.AreEqual(10, options.FailureThreshold);
            Assert.AreEqual(TimeSpan.FromMinutes(1), options.TripWaitPeriod);
            Assert.AreEqual(TimeSpan.FromSeconds(10), options.FailureExpiryPeriod);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ZeroFailureThreshold()
        {
            new CircuitBreakerSettings
            {
                FailureThreshold = 0
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ZeroWaitPeriod()
        {
            new CircuitBreakerSettings
            {
                TripWaitPeriod = TimeSpan.FromSeconds(0)
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ZeroFailureExpiryPeriod()
        {
            new CircuitBreakerSettings
            {
                FailureExpiryPeriod = TimeSpan.FromSeconds(0)
            };
        }

        [TestMethod]
        public void AddedException()
        {
            var circuitBreaker = new CircuitBreakerSettings();
            circuitBreaker.ExceptionTypes.Add(typeof(ArgumentNullException));

            Assert.IsTrue(circuitBreaker.ExceptionTypes.Contains(typeof(ArgumentNullException)));
        }
    }
}
