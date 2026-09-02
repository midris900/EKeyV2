using EKeyAuthenticationService.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace EKeyAuthenticationService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class MOHPaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<MOHPaymentController> _logger;

        public MOHPaymentController(IConfiguration configuration, IHttpClientFactory clientFactory, ILogger<MOHPaymentController> logger)
        {
            _configuration = configuration;
            _clientFactory = clientFactory;
            _logger = logger;
        }


        [HttpPost("InitiatePayment")]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentModel model)
        {
            if (ModelState.IsValid)
            {
                var paymentUrl = _configuration.GetValue<string>("PaymentService_InitiateUrl");

                var client = _clientFactory.CreateClient();


                var formData = new Dictionary<string, string>
                {
                    { "entityId", model.EntityId },
                    { "officialId", model.OfficialId },
                    { "officialIdType", model.OfficialIdType },
                    { "serviceCode", model.ServiceCode },
                    { "paymentAmount", model.PaymentAmount },
                    { "requestNumber", model.RequestNumber }
                };

                var content = new FormUrlEncodedContent(formData);

                var response = await client.PostAsync(paymentUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Error calling Payment Service.");
                }

                var serviceResponse = await response.Content.ReadAsStringAsync();

                return Ok(new
                {
                    success = true,
                    data = serviceResponse
                });

            }
            else
            {
                return BadRequest("Invalid payment request data.");
            }
        }


        [HttpGet("Response/{id}")]
        public async Task<IActionResult> PaymentResponse([FromRoute] int id, [FromQuery] string? result = null)
        {

            try
            {

                var responseUrl = _configuration.GetValue<string>("PaymentService_ResponseUrl");
                string middlewareUrl = $"{responseUrl}/{id}?result={WebUtility.UrlEncode(result)}";

                // Call the private middleware
                using var client = _clientFactory.CreateClient();
                //  client.Timeout = TimeSpan.FromSeconds(30); // Optional: set timeout

                // Call private middleware
                var response = await client.GetAsync(middlewareUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    // Deserialize JSON response
                    var middlewareResult = JsonSerializer.Deserialize<MiddlewareResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (middlewareResult?.RedirectUrl != null)
                    {
                        _logger.LogInformation($"Redirecting user to main app: {middlewareResult.RedirectUrl}");
                        // 🔥 Redirect user browser to the main application
                        return Redirect(middlewareResult.RedirectUrl);
                    }
                    else
                    {
                        _logger.LogError("RedirectUrl missing in middleware response.");
                        return StatusCode(500, "Missing RedirectUrl in middleware response.");
                    }
                }
                else
                {
                    _logger.LogError("RedirectUrl missing in middleware response.");
                    return StatusCode(500, "Missing RedirectUrl in middleware response.");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error forwarding payment response: {ex.Message}");
                return StatusCode(500, "Error processing payment response.");
            }
        }



    }


    public class MiddlewareResponse
    {
        public string Message { get; set; }
        public string RedirectUrl { get; set; }
    }

}
