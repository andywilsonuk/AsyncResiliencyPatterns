#AsyncResiliencyPatterns
Provides resiliency components (Throttle, Circuit Breaker) for async calls to external dependencies (databases calls, http requests/web services calls, IO calls, etc).

Available on NuGet at https://www.nuget.org/packages/AsyncResiliencyPatterns/

The Throttle pattern ensures that an external dependency is not flooded with more concurrent requests than it is expected to cope with.

The Circuit Breaker pattern ensures that if an external dependency starts to fail, it isn't continued to be hammered by more new requests. The circuit is configured with a level of failure tolerance within a specified time frame and if breached the circuit is 'tripped' and no further requests are send. After an amount of time, an attempt is made and if successful then 'normal' behaviour is resumed and all future requests are sent to the external dependency.

For more information on the Circuit Breaker pattern see this Netflix article http://techblog.netflix.com/2011/12/making-netflix-api-more-resilient.html

The pattern implementation allows for easy creation of ASP.Net MVC or WebAPI action filters (see Example 4).

Include the using statement so that the classes are available:
```C#
using AsyncResiliencyPatterns;
```
Example 1: Prevent more than 100 concurrent requests executing against SQL Server at any one time.
```C#
public class DatabaseThrottle
{
    private Throttle throttle = new Throttle(100);

    public async Task ExecuteQuery()
    {
        Func<Task> func = async () => { await this.ExecuteQueryInner(); };

        await this.throttle.ExecuteAsync(func);
    }

    private async Task ExecuteQueryInner()
    {
        using (SqlConnection connection = new SqlConnection())
        {
            using (SqlCommand command = new SqlCommand("SELECT * FROM BigTable", connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
```
Example 2: Breaks the circuit if 10 file writes fail in 30 seconds.
```C#
public class FileCircuitBreaker : IDisposable
{
    private CircuitBreaker circuitBreaker;

    public FileCircuitBreaker()
    {
        CircuitBreakerSettings settings = new CircuitBreakerSettings
        {
            FailureThreshold = 10,
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
```
Example 3: Breaks the circuit if 30 requests to the web service fail in 10 seconds with a maximum concurrency of 25.
```C#
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
```
Example 4: A WebAPI filter that returns 503 Service Unavailable  for all requests should the circuit to tripped.
```C#
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
