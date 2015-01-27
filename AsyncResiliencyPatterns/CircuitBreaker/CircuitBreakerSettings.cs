using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncResiliencyPatterns
{
    /// <summary>
    /// Provides a container for settings to configure a Circuit Breaker.
    /// </summary>
    public class CircuitBreakerSettings
    {
        private int failureThreshold;
        private TimeSpan tripWaitPeriod;
        private TimeSpan failureExpiryPeriod;

        public CircuitBreakerSettings()
        {
            this.FailureThreshold = 10;
            this.TripWaitPeriod = TimeSpan.FromMinutes(1);
            this.FailureExpiryPeriod = TimeSpan.FromSeconds(10);
            this.ExceptionTypes = new List<Type>();
        }

        /// <summary>
        /// Gets or sets how many failures are required to trip the circuit breaker.
        /// </summary>
        /// <remarks>Default: 10. Must be great than zero.</remarks>
        public int FailureThreshold
        {
            get { return this.failureThreshold; }
            set 
            {
                if (value < 1) throw new ArgumentException("The threshold cannot be less than one.", "value");
                this.failureThreshold = value;
            }
        }

        /// <summary>
        /// Gets or sets the period to wait once the circuit breaker has tripped.
        /// </summary>
        /// <remarks>Default: 00:01:00. Must be great than zero.</remarks>
        public TimeSpan TripWaitPeriod
        {
            get { return this.tripWaitPeriod; }
            set
            {
                if (value <= TimeSpan.Zero) throw new ArgumentException("The trip wait period cannot be less than or equal to zero.", "value");
                this.tripWaitPeriod = value;
            }
        }

        /// <summary>
        /// Gets or sets the period since the last failure before the count is reset.
        /// </summary>
        /// <remarks>Default: 00:00:10. Must be great than zero.</remarks>
        public TimeSpan FailureExpiryPeriod
        {
            get { return this.failureExpiryPeriod; }
            set
            {
                if (value <= TimeSpan.Zero) throw new ArgumentException("The failure expiry period cannot be less than or equal to zero.", "value");
                this.failureExpiryPeriod = value;
            }
        }

        /// <summary>
        /// Gets a list of exceptions that increment the failure count. 
        /// </summary>
        /// <remarks>
        /// The default behaviour is that all exceptions increment the count.
        /// </remarks>
        public ICollection<Type> ExceptionTypes { get; private set; }
    }
}
