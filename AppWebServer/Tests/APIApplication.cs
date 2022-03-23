namespace AppWebServer.Tests;

using AppWebServer.Tests;
using AppWebServer.Tests.XUnitLogger;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

public class APIApplication: WebApplicationFactory<Program>
{
    public APIApplication(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
    }

    private ITestOutputHelper OutputHelper { get; }
    protected override IHost CreateHost(IHostBuilder builder)
    { 
        builder.ConfigureServices(services => 
        {
            services.AddSingleton<ICircuitBreaker,CircuitBreakerWithMonkeyCaos>();
        });
        builder.ConfigureLogging(loggingBuilder =>
        {
            loggingBuilder.Services.AddSingleton<ILoggerProvider>(serviceProvider => new XUnitLoggerProvider(OutputHelper));
        });
        
        return base.CreateHost(builder);
    }
}