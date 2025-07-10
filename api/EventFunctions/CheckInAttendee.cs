using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace EventFunctions
{
    public class CheckInAttendee
    {
        private readonly ILogger _logger;

        public CheckInAttendee(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CheckInAttendee>();
        }

        [Function("CheckInAttendee")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "checkin")] HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("{\"message\": \"Check-in complete!\"}");
            return response;
        }
    }
}
