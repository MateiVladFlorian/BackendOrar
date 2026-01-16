using BackendOrar.Models;
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
    public class MetadataController : ControllerBase
    {
        readonly IMetadataService metadataService;

        public MetadataController(IMetadataService metadataService)
        { this.metadataService = metadataService; }

        [HttpGet, AllowAnonymous]
        public IActionResult GetMetadataModels()
        {
            MetadataModel[] models = metadataService.GetMetadataModels();
            return Ok(models);
        }
    }
}
