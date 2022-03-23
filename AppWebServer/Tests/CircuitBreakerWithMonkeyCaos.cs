namespace AppWebServer.Tests;

using System.Text.Json;
using AppWebServer.ResiliencePolicy;
using Polly.CircuitBreaker;
using Polly.Contrib.Simmy;
using Polly.Contrib.Simmy.Outcomes;
using Polly.Wrap;

class CircuitBreakerWithMonkeyCaos : ICircuitBreaker
{
    private readonly ILogger<ICircuitBreaker> _logger;
    private AsyncCircuitBreakerPolicy _instance;
    private readonly IConfiguration _configuration;
    private readonly AsyncPolicyWrap _policyWrap;
    public CircuitBreakerWithMonkeyCaos(ILogger<ICircuitBreaker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _policyWrap = new CircuitBreaker(_logger).GetInstance();
    }
    public AsyncPolicyWrap GetInstance()
    {
        var monkeyCaosEnable = _configuration.GetValue<bool>("MonkeyCaosEnable");
        var monkeyCaosInjectionRate = _configuration.GetValue<double>("MonkeyCaosInjectionRate");

        return _policyWrap.WrapAsync(
            MonkeyPolicy.InjectExceptionAsync(
            with => with.Fault(new Exception("Erro gerado em simulação de caos com Simmy..."))
                .InjectionRate(JsonSerializer.Deserialize<double>(monkeyCaosInjectionRate))
                .Enabled(monkeyCaosEnable)));
    }
}