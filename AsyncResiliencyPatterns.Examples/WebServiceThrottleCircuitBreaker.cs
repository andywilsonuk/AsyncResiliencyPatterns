using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Formatting;

namespace AsyncResiliencyPatterns.Examples
{
    /// <summary>
    /// Example 3: Breaks the circuit if 30 requests to the web service fail in 10 seconds with a maximum concurrency of 25.
    /// </summary>
    public class WebServiceThrottleCircuitBreaker : IDisposable
    {
        private Throttle throttle;
        private CircuitBreaker circuitBreaker;

        public WebServiceThrottleCircuitBreaker()
        {
            CircuitBreakerSettings settings = new CircuitBreakerSettings
            {
                FailureThreshold = 30,
                FailureExpiryPeriod = TimeSpan.FromSeconds(10),
                TripWaitPeriod = TimeSpan.FromSeconds(10),
            };
            // only increment the failure count on an HttpRequest exception
            settings.ExceptionTypes.Add(typeof(HttpRequestException));
            this.circuitBreaker = new CircuitBreaker(settings);
            // embed the circuit breaker inside the throttle
            this.throttle = new Throttle(25, this.circuitBreaker);
        }

        public async Task PostWebService(object payload)
        {
            Func<Task> func = async () =>
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.PostAsJsonAsync("http://somewhere.org/endpoint", payload);
                    response.EnsureSuccessStatusCode();
                }
            };

            await this.throttle.ExecuteAsync(func);
        }

        public void Dispose()
        {
            this.circuitBreaker.Dispose();
        }
    }
}
