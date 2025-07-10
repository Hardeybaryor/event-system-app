using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace EventFunctions
{
    public class RegisterAttendee
    {
        private readonly ILogger _logger;

        public RegisterAttendee(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RegisterAttendee>();
        }

        [Function("RegisterAttendee")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "register")] HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("{\"message\": \"Registered!\"}");
            return response;
        }
    }
}
