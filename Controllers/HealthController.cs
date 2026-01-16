using BackendOrar.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace BackendOrar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("DefaultPolicy")]
    public class HealthController : ControllerBase
    {
        readonly OrarContext orarContext;
        readonly ILogger<HealthController> logger;

        public HealthController(OrarContext orarContext, ILogger<HealthController> logger)
        {
            this.orarContext = orarContext;
            this.logger = logger;
        }

        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                /* check database connection */
                var dbConnected = await orarContext.Database.CanConnectAsync();

                if (!dbConnected)
                {
                    logger.LogWarning("Database connection failed.");
                    return StatusCode(503, new
                    {
                        Status = "Unhealthy",
                        Timestamp = DateTime.UtcNow,
                        Database = "Disconnected"
                    });
                }

                return Ok(new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Database = "Connected",
                    Uptime = GetUptime()
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Health check failed.");
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Error = ex.Message
                });
            }
        }

        private static string GetUptime()
        {
            var uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();
            return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
        }
    }
}
