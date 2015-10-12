using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncResiliencyPatterns.Examples
{
    /// <summary>
    /// Example 2: Breaks the circuit if 10 file writes fail in 30 seconds.
    /// </summary>
    public class FileCircuitBreaker : IDisposable
    {
        private CircuitBreaker circuitBreaker;

        public FileCircuitBreaker()
        {
            CircuitBreakerSettings settings = new CircuitBreakerSettings
            {
                FailureThreshold = 1,
                FailureExpiryPeriod = TimeSpan.FromSeconds(30),
                TripWaitPeriod = TimeSpan.FromMinutes(1),
            };
            // only increment the failure count on an IO exception
            settings.ExceptionTypes.Add(typeof(IOException));
            this.circuitBreaker = new CircuitBreaker(settings);
        }

        public async Task WriteFile()
        {
            Func<Task> func = async () =>
            {
                using (var writer = File.CreateText(Guid.NewGuid().ToString()))
                {
                    await writer.WriteLineAsync("text");
                }
            };                      

            await this.circuitBreaker.ExecuteAsync(func);
        }

        public void Dispose()
        {
            this.circuitBreaker.Dispose();
        }
    }
}
