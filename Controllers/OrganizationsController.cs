using AllSet.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllSet.Controllers
{
    [Route("api/[controller]")]
    public class OrganizationsController : Controller
    {
        private AllSetDbContext _dbContext;

        public OrganizationsController(AllSetDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _dbContext.Organizations.ToListAsync();
            return Ok(result);
        }

    }
}

