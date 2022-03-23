using Polly;
using Polly.CircuitBreaker;
using Polly.Wrap;

namespace AppWebServer.ResiliencePolicy;
public class CircuitBreaker: ICircuitBreaker
{
    private readonly ILogger<ICircuitBreaker> _logger;
    private AsyncPolicyWrap _instance;
    public CircuitBreaker(ILogger<ICircuitBreaker> logger)
    {
        _logger = logger;
    }

    public AsyncPolicyWrap GetInstance(){
        if(_instance == null){
            _instance = CreatePolicy();
        }
        return _instance;
    }

    private AsyncPolicyWrap CreatePolicy()
    {

        return Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(15),
                onBreak: (_, _) =>
                {
                    _logger.LogError($"# {DateTime.Now:HH:mm:ss} # Open (onBreak)");
                },                            
                onReset: () =>
                {
                    _logger.LogInformation($"# {DateTime.Now:HH:mm:ss} # Closed (onReset)");
                },
                onHalfOpen: () =>
                {
                    _logger.LogWarning($"# {DateTime.Now:HH:mm:ss} # Half Open (onHalfOpen)");
                })
            .WrapAsync(Policy.BulkheadAsync(12));
    }
}