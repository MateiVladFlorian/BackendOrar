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
    public class GroupController : ControllerBase
    {
        readonly IGroupService groupService;
        readonly ILogger<GroupController> logger;

        public GroupController(IGroupService groupService, ILogger<GroupController> logger)
        {
            this.groupService = groupService;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> GetCoursesAsync([FromForm] GroupFilterModel? model)
        {
            Group[] groups = await groupService.GetGroups(model);
            logger.LogInformation("Group entries count: {0}.", groups.Length);
            return Ok(groups);
        }

        [HttpPost("create"), Authorize]
        public async Task<IActionResult> AddCourseAsync([FromForm] GroupRequestModel? model)
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
            var (status, group, message) = await groupService.AddGroup(accessToken, model);
            string[] errors = ["401", "400", "500", "409", "201"];
            logger.LogWarning($"Status code {errors[status + 3]}: {message}");

            return status switch
            {
                -3 => Unauthorized(new ApiResponse<object>(401, message)),
                -2 => BadRequest(new ApiResponse<object>(400, message)),
                -1 => StatusCode(500, new ApiResponse<object>(500, message)),
                0 => Conflict(new ApiResponse<Group>(409, message, group)),
                1 => Created(),
                _ => StatusCode(500, new ApiResponse<object>(500, "Unexpected error occurred."))
            };
        }
    }
}
