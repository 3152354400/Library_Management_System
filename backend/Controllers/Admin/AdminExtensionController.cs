using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using backend.Services.Admin;
using backend.Services.Web;
using backend.Models;
using backend.DTOs.ReaderDtos;   // PayFineRequest

namespace backend.Controllers.Admin
{
    /// <summary>
    /// 管理员扩展功能控制器
    /// 包含：罚款管理、荐购处理等功能
    /// </summary>
    [ApiController]
    [Route("api/admin/[controller]")]
    public class AdminExtensionController : ControllerBase
    {
        private readonly AdminFineService _fineService;
        private readonly AdminRecommendService _recommendService;
        private readonly SecurityService _securityService;

        public AdminExtensionController(
            AdminFineService fineService,
            AdminRecommendService recommendService,
            SecurityService securityService)
        {
            _fineService = fineService;
            _recommendService = recommendService;
            _securityService = securityService;
        }

        private int? GetCurrentLibrarianId()
        {
            var loginUser = _securityService.GetLoginUser();
            if (_securityService.CheckIsLibrarian(loginUser))
            {
                var librarian = loginUser.User as Librarian;
                return (int)librarian.LibrarianID;
            }
            return null;
        }

        #region 罚款管理

        /// <summary>
        /// 获取所有待缴罚款
        /// </summary>
        [HttpGet("fines/pending")]
        public async Task<ActionResult> GetPendingFines()
        {
            var fines = await _fineService.GetAllPendingFinesAsync();
            return Ok(fines);
        }

        /// <summary>
        /// 缴纳罚款
        /// </summary>
        [HttpPost("fines/pay")]
        public async Task<IActionResult> PayFine([FromBody] PayFineRequest request)
        {
            var adminId = GetCurrentLibrarianId();
            if (adminId == null)
                return Unauthorized(new { message = "请以管理员身份登录" });

            var message = await _fineService.PayFineAsync(
                request.FineId, 
                request.PayAmount, 
                request.PayMethod, 
                adminId.Value);
            return Ok(new { message });
        }

        /// <summary>
        /// 减免罚款请求
        /// </summary>
        public class WaiveFineRequest
        {
            public int FineId { get; set; }
            public string Reason { get; set; }
        }

        /// <summary>
        /// 减免罚款
        /// </summary>
        [HttpPost("fines/waive")]
        public async Task<IActionResult> WaiveFine([FromBody] WaiveFineRequest request)
        {
            var adminId = GetCurrentLibrarianId();
            if (adminId == null)
                return Unauthorized(new { message = "请以管理员身份登录" });

            var success = await _fineService.WaiveFineAsync(request.FineId, adminId.Value, request.Reason);
            return Ok(new { success });
        }

        #endregion

        #region 荐购管理

        /// <summary>
        /// 获取待处理荐购
        /// </summary>
        [HttpGet("recommends/pending")]
        public async Task<ActionResult> GetPendingRecommends()
        {
            var recommends = await _recommendService.GetPendingRecommendsAsync();
            return Ok(recommends);
        }

        /// <summary>
        /// 处理荐购请求
        /// </summary>
        public class HandleRecommendRequest
        {
            public int RecommendId { get; set; }
            public string Action { get; set; }  // "采纳" 或 "拒绝"
            public string HandleResult { get; set; }
            public decimal? PurchasePrice { get; set; }
        }

        /// <summary>
        /// 处理荐购
        /// </summary>
        [HttpPost("recommends/handle")]
        public async Task<IActionResult> HandleRecommend([FromBody] HandleRecommendRequest request)
        {
            var adminId = GetCurrentLibrarianId();
            if (adminId == null)
                return Unauthorized(new { message = "请以管理员身份登录" });

            var message = await _recommendService.HandleRecommendAsync(
                request.RecommendId,
                adminId.Value,
                request.Action,
                request.HandleResult,
                request.PurchasePrice);
            return Ok(new { message });
        }

        #endregion
    }
}
