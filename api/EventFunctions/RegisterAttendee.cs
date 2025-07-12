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
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
        }

        [Function("RegisterAttendee")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "register")] HttpRequestData req)
        {
            var response = req.CreateResponse();
            response.Headers.Add("Content-Type", "application/json");

            try
            {
                _logger.LogInformation("Reading request body...");
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonSerializer.Deserialize<RegistrationDto>(body);

                _logger.LogInformation("Generating token...");
                string token = Guid.NewGuid().ToString();

                _logger.LogInformation("Connecting to SQL database...");
                var sqlConn = Environment.GetEnvironmentVariable("SqlConnectionString");
                using (var conn = new SqlConnection(sqlConn))
                {
                    await conn.OpenAsync();
                    var cmd = new SqlCommand(
                        "INSERT INTO Attendees (EventID, FirstName, LastName, Email, QRCodeToken) " +
                        "VALUES (@e, @f, @l, @m, @t)", conn);
                    cmd.Parameters.AddWithValue("@e", data.EventId);
                    cmd.Parameters.AddWithValue("@f", data.FirstName);
                    cmd.Parameters.AddWithValue("@l", data.LastName);
                    cmd.Parameters.AddWithValue("@m", data.Email);
                    cmd.Parameters.AddWithValue("@t", token);
                    await cmd.ExecuteNonQueryAsync();
                }

                _logger.LogInformation("Generating QR code...");
                using var qrGen = new QRCodeGenerator();
                var qrData = qrGen.CreateQrCode(token, QRCodeGenerator.ECCLevel.Q);
                using var qr = new PngByteQRCode(qrData);
                byte[] pngBytes = qr.GetGraphic(20);

                _logger.LogInformation("Uploading QR to Blob Storage...");
                var blobConn = Environment.GetEnvironmentVariable("StorageConnectionString");
                var blobService = new BlobServiceClient(blobConn);
                var container = blobService.GetBlobContainerClient("qrcodes");
                await container.CreateIfNotExistsAsync();
                var blob = container.GetBlobClient($"{token}.png");
                await blob.UploadAsync(new BinaryData(pngBytes), overwrite: true);
                string qrUrl = blob.Uri.ToString();

                _logger.LogInformation("Function completed successfully.");
                var result = new { token, qrCodeUrl = qrUrl };
                await response.WriteStringAsync(JsonSerializer.Serialize(result));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex.Message}");
                response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                return response;
            }
        }
    }
}
