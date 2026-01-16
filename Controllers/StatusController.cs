using BackendOrar.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BackendOrar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("DefaultPolicy")]
    public class StatusController : ControllerBase
    {
        private readonly ILogger<StatusController> logger;

        public StatusController(ILogger<StatusController> logger)
        { this.logger = logger; }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetStatus()
        {
            logger.LogInformation("Status endpoint called.");

            return Ok(new
            {
                Status = "Online",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                Server = Environment.MachineName,
                OS = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                Framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
            });
        }
    }
}
