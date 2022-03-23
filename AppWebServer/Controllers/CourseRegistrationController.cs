using System.Net;
using System.Text.Json;
using AppWebServer.Models;
using AppWebServer.ResiliencePolicy;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class CourseRegistrationController
{
    private readonly ILogger<CourseRegistrationController> _logger;
    private ICircuitBreaker _resiliencePolicy;
    private readonly IConfiguration _configuration;

    public CourseRegistrationController(
            ILogger<CourseRegistrationController> logger,
            ICircuitBreaker resiliencePolicy,
            IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _resiliencePolicy = resiliencePolicy;
    }

    [HttpPost(Name = "PostCourseRegistry")]
    public async Task<ActionResult<CourseRegistration>> Post(CourseRegistration registry)
    {
        var circuitBreaker = _resiliencePolicy.GetInstance();
        try{
            var result = await circuitBreaker.ExecuteAsync<CourseRegistration>(async () =>
            {
                var connectionString = _configuration.GetConnectionString("StorageConnectionString");
                var queueName = _configuration.GetValue<string>("QueueName");
                QueueClient queueClient = new QueueClient(connectionString, queueName, new QueueClientOptions()
                    {
                        Retry = {
                            Delay = TimeSpan.FromSeconds(0),
                            MaxDelay = TimeSpan.FromSeconds(1),
                            MaxRetries = 0
                        }
                    });
                await queueClient.CreateIfNotExistsAsync();
                if (queueClient.Exists())
                {
                    await queueClient.SendMessageAsync(JsonSerializer.Serialize(registry));
                    _logger.LogInformation($"* {DateTime.Now:HH:mm:ss} * SUCCESS Student {registry.Name} Course {registry.Course}");
                }
                return registry;
            });
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"# {DateTime.Now:HH:mm:ss} # | Falha ao invocar Queue | {ex.Message}");
        }
        return new StatusCodeResult(500);
    }
}
