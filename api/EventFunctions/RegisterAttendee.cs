using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventFunctions;

public class RegisterAttendee
{
    private readonly ILogger<RegisterAttendee> _logger;

    public RegisterAttendee(ILogger<RegisterAttendee> logger)
    {
        _logger = logger;
    }

    [Function("RegisterAttendee")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}
