using AllSet.Domain;
using AllSet.Services;
using Microsoft.AspNetCore.Mvc;

namespace AllSet.Controllers
{
    [Route("api/[controller]")]
    public class AvailabalityController : Controller
    {
        private AllSetDbContext _dbContext;
        private readonly AvailabilityService _availabilityService;


        public AvailabalityController(AllSetDbContext dbContext,
            AvailabilityService availabilityService)
        {
            _dbContext = dbContext;
            _availabilityService = availabilityService;
        }

        [HttpGet("api/resources/{resourceId}/[controller]")]
        public async Task<IActionResult> GetAvailabilityOld(Guid resourceId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var availability = await _availabilityService.GetAvailability(resourceId, startDate, endDate);
            return Ok(availability);
        }

        [HttpGet("/api/resources/{resourceId}/[controller]")]
        public async Task<IActionResult> GetAvailability(Guid resourceId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var availability = await _availabilityService.GetAvailability(resourceId, startDate, endDate);
            return Ok(availability);
        }

    }
}

