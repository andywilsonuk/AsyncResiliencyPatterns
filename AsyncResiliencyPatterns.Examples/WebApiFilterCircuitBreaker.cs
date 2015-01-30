using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Formatting;
using System.Net;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Threading;
using System.Web.Http;

namespace AsyncResiliencyPatterns.Examples
{
    /// <summary>
    /// Example 4: A WebAPI filter that returns 503 Service Unavailable for all requests should the circuit to tripped.
    /// </summary>
    public class WebApiFilterCircuitBreaker : IActionFilter
    {
        private CircuitBreaker circuitBreaker;

        public WebApiFilterCircuitBreaker()
        {
            this.circuitBreaker = new CircuitBreaker(new CircuitBreakerSettings());
        }

        public bool AllowMultiple
        {
            get { return false; }
        }

        public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            try
            {
                return await this.circuitBreaker.ExecuteAsync(continuation);
            }
            catch (CircuitBreakerTrippedException)
            {
                throw new HttpResponseException(HttpStatusCode.ServiceUnavailable);
            }
        }

        public void Dispose()
        {
            this.circuitBreaker.Dispose();
        }
    }
}
