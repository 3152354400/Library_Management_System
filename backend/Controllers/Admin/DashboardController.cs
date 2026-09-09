using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using backend.DTOs.Admin;
using backend.Services.Admin;

namespace backend.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _service;

        public DashboardController(DashboardService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardStatsDto>> GetStats()
        {
            return Ok(await _service.GetStatsAsync());
        }
    }
}
