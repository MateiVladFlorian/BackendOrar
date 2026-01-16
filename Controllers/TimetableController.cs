using BackendOrar.Data;
using BackendOrar.Models;
using BackendOrar.Models.Filters;
using BackendOrar.Responses;
using BackendOrar.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BackendOrar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("DefaultPolicy")]
    public class TimetableController : ControllerBase
    {
        readonly ITimetableService timetableService;
        readonly ILogger<TimetableController> logger;

        public TimetableController(ITimetableService timetableService, 
            ILogger<TimetableController> logger)
        {
            this.timetableService = timetableService;
            this.logger = logger;
        }

        [HttpGet("{id?}")]
        public async Task<IActionResult> GetTimetableEntries(int? id)
        {
            TimetableEntry[] entries = await timetableService.GetTimetableEntries(id);
            logger.LogInformation("Timetable entries count: {0}.", entries.Length);
            return Ok(entries);
        }

        [HttpGet]
        public async Task<IActionResult> GetTimetableFilteredEntries([FromQuery]TimetableFilterModel model)
        {
            TimetableEntry[] entries = await timetableService.GetFilteredTimetableEntries(model);
            logger.LogInformation("Timetable entries count: {0}.", entries.Length);
            return Ok(entries);
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> AddTimetableEntryAsync([FromForm]TimetableRequestModel? model)
        {
            if (!HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                logger.LogWarning("Authorization header is missing.");
                return Unauthorized(new ApiResponse<object>(401, "Authorization header is missing."));
            } 

            var tokenParts = authHeader.ToString().Split(' ');
            if (tokenParts.Length != 2 || !tokenParts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("Invalid authorization format. Use '{0}'.", "Bearer {token}");
                return Unauthorized(new ApiResponse<object>(401, "Invalid authorization format. Use 'Bearer {token}'."));
            }

            string accessToken = tokenParts[1];
            var (status, entry, message) = await timetableService.AddTimetableEntry(accessToken, model);
            string[] errors = ["401", "400", "500", "409", "201"];
            logger.LogWarning($"Status code {errors[status + 3]}: {message}");

            return status switch
            {
                -3 => Unauthorized(new ApiResponse<object>(401, message)),
                -2 => BadRequest(new ApiResponse<object>(400, message)),
                -1 => StatusCode(500, new ApiResponse<object>(500, message)),
                0 => Conflict(new ApiResponse<Timetable>(409, message, entry)),
                1 => Created(),
                _ => StatusCode(500, new ApiResponse<object>(500, "Unexpected error occurred."))
            };
        }

        [HttpPut("{id}"), Authorize]
        public async Task<IActionResult> UpdateTimetableEntryAsync(int id, [FromForm]TimetableUpdateModel? model)
        {
            if (!HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                logger.LogWarning("Authorization header is missing.");
                return Unauthorized(new { message = "Authorization header is missing." });
            }

            var tokenParts = authHeader.ToString().Split(' ');
            if (tokenParts.Length != 2 || !tokenParts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("Invalid authorization format. Use '{0}'.", "Bearer {token}");
                return Unauthorized(new { message = "Invalid authorization format. Use 'Bearer {token}'." });
            }

            string accessToken = tokenParts[1];
            var (status, error_message) = await timetableService.UpdateTimetableEntry(id, accessToken, model);
            string[] errors = ["401", "400", "409", "404", "204", "500"];
            logger.LogWarning($"Status code {errors[status + 3]}: {error_message}");

            return status switch
            {
                -3 => Unauthorized(new { message = error_message }),
                -2 => BadRequest(new { message = error_message }),
                -1 => Conflict( new { message = error_message }),
                0 => NotFound(new { message = error_message }),
                1 => NoContent(),
                _ => StatusCode(500, new ApiResponse<object>(500, "Unexpected error occurred."))
            };
        }

        [HttpDelete("{id}"), Authorize]
        public async Task<ActionResult> DeleteTimetableEntryAsync(int id)
        {
            if (!HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                logger.LogWarning("Authorization header is missing.");
                return Unauthorized(new { message = "Authorization header is missing." });
            }

            var tokenParts = authHeader.ToString().Split(' ');
            if (tokenParts.Length != 2 || !tokenParts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("Invalid authorization format. Use '{0}'.", "Bearer {token}");
                return Unauthorized(new { message = "Invalid authorization format. Use 'Bearer {token}'." });
            }

            string accessToken = tokenParts[1];
            var (status, detailedMessage) = await timetableService.DeleteTimetableEntry(accessToken, id);
            string[] errors = ["401", "404", "204", "500"];
            logger.LogWarning($"Status code {errors[status + 2]}: {detailedMessage}");

            return status switch
            {
                -2 => Unauthorized(new { message = detailedMessage }),
                -1 => NotFound(new { message = "Timetable entry not found." }),
                0 => StatusCode(500, new { message = "Database error occurred." }),
                1 => NoContent(),
                _ => StatusCode(500, new { message = "Unexpected error occured." })
            };
        }
    }
}
