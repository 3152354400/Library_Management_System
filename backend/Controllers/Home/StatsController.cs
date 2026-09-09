using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using backend.Repositories.Admin;

namespace backend.Controllers.Home
{
    [ApiController]
    [Route("api/stats")]
    public class StatsController : ControllerBase
    {
        private readonly DashboardRepository _repository;

        public StatsController(DashboardRepository repository)
        {
            _repository = repository;
        }

        // 前台首页馆藏数据（公开，无需登录）
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            return Ok(new
            {
                totalBooks = await _repository.ScalarIntAsync("SELECT COUNT(*) FROM BOOK"),
                totalReaders = await _repository.ScalarIntAsync("SELECT COUNT(*) FROM READER"),
                monthBorrows = await _repository.ScalarIntAsync("SELECT COUNT(*) FROM BORROWRECORD WHERE BORROWTIME >= TRUNC(SYSDATE, 'MM')"),
                seatAvailable = await _repository.ScalarIntAsync("SELECT COUNT(*) FROM SEAT WHERE RESERVATIONSTATUS = '空闲'")
            });
        }
    }
}
