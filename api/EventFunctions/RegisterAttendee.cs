using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using QRCoder;

namespace EventFunctions
{
    public class RegisterAttendee
    {
        private readonly ILogger _logger;

        public RegisterAttendee(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RegisterAttendee>();
        }

        public class RegistrationDto
        {
            public int EventId { get; set; }
            public required string FirstName { get; set; }
            public required string LastName { get; set; }
            public required string Email { get; set; }
        }

        [Function("RegisterAttendee")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "register")] HttpRequestData req)
        {
            var response = req.CreateResponse();
            response.Headers.Add("Content-Type", "application/json");

            string step = "Initializing";

            try
            {
                step = "Reading request body";
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                _logger.LogInformation($"Request body: {body}");
                var data = JsonSerializer.Deserialize<RegistrationDto>(body);

                if (data == null ||
                    string.IsNullOrEmpty(data.FirstName) ||
                    string.IsNullOrEmpty(data.LastName) ||
                    string.IsNullOrEmpty(data.Email) ||
                    data.EventId <= 0)
                {
                    throw new Exception("Invalid or incomplete registration data.");
                }

                step = "Generating token";
                string token = Guid.NewGuid().ToString();
                int eventId = data.EventId;

                step = "Connecting to SQL";
                var sqlConn = Environment.GetEnvironmentVariable("SqlConnectionString");

                using (var conn = new SqlConnection(sqlConn))
                {
                    await conn.OpenAsync();
                    var cmd = new SqlCommand(
                        "INSERT INTO Attendees (EventID, FirstName, LastName, Email, QRCodeToken) " +
                        "VALUES (@e, @f, @l, @m, @t)", conn);
                    cmd.Parameters.AddWithValue("@e", eventId);
                    cmd.Parameters.AddWithValue("@f", data.FirstName);
                    cmd.Parameters.AddWithValue("@l", data.LastName);
                    cmd.Parameters.AddWithValue("@m", data.Email);
                    cmd.Parameters.AddWithValue("@t", token);
                    await cmd.ExecuteNonQueryAsync();
                }

                step = "Generating QR code";
                using var qrGen = new QRCodeGenerator();
                var qrData = qrGen.CreateQrCode(token, QRCodeGenerator.ECCLevel.Q);
                using var qr = new PngByteQRCode(qrData);
                byte[] pngBytes = qr.GetGraphic(20);

                step = "Uploading to Blob";
                var blobConn = Environment.GetEnvironmentVariable("StorageConnectionString");
                var blobService = new BlobServiceClient(blobConn);
                var container = blobService.GetBlobContainerClient("qrcodes");
                await container.CreateIfNotExistsAsync();
                var blob = container.GetBlobClient($"{token}.png");
                await blob.UploadAsync(new BinaryData(pngBytes), overwrite: true);
                string qrUrl = blob.Uri.ToString();

                step = "Success";
                var result = new { step, token, qrCodeUrl = qrUrl };
                await response.WriteStringAsync(JsonSerializer.Serialize(result));
                return response;
            }
            catch (Exception ex)
            {
                var error = new { step, error = ex.Message };
                response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                await response.WriteStringAsync(JsonSerializer.Serialize(error));
                return response;
            }
        }
    }
}
