using AllSet.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static AllSet.DTOs.DataTransferObjects;

namespace AllSet.Controllers
{
    [Route("api/[controller]")]
    public class ResourcesController : Controller
    {
        private AllSetDbContext _dbContext;

        public ResourcesController(AllSetDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("/api/organizations/{orgId}/[controller]")]
        public async Task<IActionResult> GetOrganizationResources(Guid orgId)
        {
            var result = await _dbContext.Resources.Where(x => x.OrganizationId == orgId).ToListAsync();
            return Ok(result);
        }

        [HttpPost("/api/organizations/{orgId}/[controller]")]
        public async Task<IActionResult> CreateResource(Guid orgId, [FromBody] CreateResourceRequestDto dto)
        {
            Resource resource = new()
            {
                Name = dto.Name,
                OrganizationId = orgId,
                CreatedOn = DateTime.UtcNow
            };
            await _dbContext.Resources.AddAsync(resource);
            await _dbContext.SaveChangesAsync();
            return Ok(resource);
        }


    }
}

