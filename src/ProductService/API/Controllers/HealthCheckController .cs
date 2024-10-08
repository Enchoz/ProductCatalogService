using Microsoft.AspNetCore.Mvc;
using ProductService.Domain.Entities;
using ProductService.Services;
using ProductService.Services.Interfaces;

namespace ProductService.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
        }
    }
}