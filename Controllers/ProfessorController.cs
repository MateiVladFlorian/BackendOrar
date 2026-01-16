using BackendOrar.Data;
using BackendOrar.Models;
using BackendOrar.Models.Filters;
using BackendOrar.Responses;
using BackendOrar.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.RegularExpressions;

namespace BackendOrar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("DefaultPolicy")]
    public class ProfessorController : ControllerBase
    {
        readonly IProfessorService professorService;
        readonly ILogger<ProfessorController> logger;

        public ProfessorController(IProfessorService professorService, ILogger<ProfessorController> logger)
        {
            this.professorService = professorService;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> GetProfessorsAsync([FromForm] ProfessorFilterModel? model)
        {
            Professor[] professors = await professorService.GetProfessors(model);
            logger.LogInformation("Professor entries count: {0}.", professors.Length);
            return Ok(professors);
        }

        [HttpPost("create"), Authorize]
        public async Task<IActionResult> AddProfessorAsync([FromForm] ProfessorRequestModel? model)
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
            var (status, professor, message) = await professorService.AddProfessor(accessToken, model);
            string[] errors = ["401", "400", "500", "409", "201"];
            logger.LogWarning($"Status code {errors[status + 3]}: {message}");

            return status switch
            {
                -3 => Unauthorized(new ApiResponse<object>(401, message)),
                -2 => BadRequest(new ApiResponse<object>(400, message)),
                -1 => StatusCode(500, new ApiResponse<object>(500, message)),
                0 => Conflict(new ApiResponse<Professor>(409, message, professor)),
                1 => Created(),
                _ => StatusCode(500, new ApiResponse<object>(500, "Unexpected error occurred."))
            };
        }
    }
}
