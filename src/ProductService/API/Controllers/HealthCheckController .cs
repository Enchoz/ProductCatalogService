using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ProductService.API.Controllers
{
    /// <summary>
    /// Controller for performing health checks on the service.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class HealthCheckController : ControllerBase
    {
        /// <summary>
        /// Performs a health check on the service.
        /// </summary>
        /// <returns>The current health status of the service.</returns>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Perform a health check",
            Description = "Returns the current health status of the service along with a timestamp.",
            Tags = new[] { "Health" }
        )]
        [SwaggerResponse(200, "Service is healthy", typeof(HealthCheckResponse))]
        public IActionResult Get()
        {
            var response = new HealthCheckResponse
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow
            };
            return Ok(response);
        }
    }

    /// <summary>
    /// Represents the response of a health check.
    /// </summary>
    public class HealthCheckResponse
    {
        /// <summary>
        /// The current status of the service.
        /// </summary>
        /// <example>Healthy</example>
        public string Status { get; set; }

        /// <summary>
        /// The UTC timestamp when the health check was performed.
        /// </summary>
        /// <example>2024-10-10T12:00:00Z</example>
        public DateTime Timestamp { get; set; }
    }
}