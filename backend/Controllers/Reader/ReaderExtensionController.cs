using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using backend.Services.ReaderService;
using backend.Services.Web;
using backend.DTOs.ReaderDtos;
using backend.Models;

namespace backend.Controllers.ReaderFeature
{
    /// <summary>
    /// 读者扩展功能控制器
    /// 包含：续借、预约、荐购、消息通知等功能
    /// </summary>
    [ApiController]
    [Route("api/reader/extension")]
    public class ReaderExtensionController : ControllerBase
    {
        private readonly IReaderExtensionService _service;
        private readonly SecurityService _securityService;

        public ReaderExtensionController(
            IReaderExtensionService service,
            SecurityService securityService)
        {
            _service = service;
            _securityService = securityService;
        }

        /// <summary>
        /// 获取当前读者ID
        /// </summary>
        private int? GetCurrentReaderId()
        {
            var loginUser = _securityService.GetLoginUser();
            if (_securityService.CheckIsReader(loginUser))
            {
                var reader = loginUser.User as Reader;
                return (int)reader.ReaderID;
            }
            return null;
        }

        #region 续借管理

        /// <summary>
        /// 续借图书
        /// </summary>
        /// <param name="request">续借请求</param>
        /// <returns>续借结果</returns>
        [HttpPost("renew")]
        public async Task<IActionResult> RenewBook([FromBody] RenewBookRequest request)
        {
            var readerId = GetCurrentReaderId();
            if (readerId == null)
                return Unauthorized(new { message = "请先登录" });

            var result = await _service.RenewBookAsync(readerId.Value, request.BookId);
            return Ok(result);
        }

        #endregion

        #region 罚款管理

        /// <summary>
        /// 获取罚款列表（分页）
        /// </summary>
        [HttpGet("fines/paged")]
        public async Task<IActionResult> GetFinesPaged(
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageNum = 1,
            [FromQuery] string status = null)
        {
            var readerId = GetCurrentReaderId();
            if (readerId == null)
                return Unauthorized(new { message = "请先登录" });

            var result = await _service.GetReaderFinesPagedAsync(readerId.Value, pageSize, pageNum, status);
            return Ok(result);
        }

        /// <summary>
        /// 获取罚款列表
        /// </summary>
        /// <returns>罚款记录列表</returns>
        [HttpGet("fines")]
        public async Task<IActionResult> GetFines()
        {
            var readerId = GetCurrentReaderId();
            if (readerId == null)
                return Unauthorized(new { message = "请先登录" });

            var fines = await _service.GetReaderFinesAsync(readerId.Value);
            return Ok(fines);
        }

        /// <summary>
        /// 获取罚款汇总
        /// </summary>
        /// <returns>罚款汇总信息</returns>
        [HttpGet("fines/summary")]
        public async Task<IActionResult> GetFineSummary()
        {
            var readerId = GetCurrentReaderId();
            if (readerId == null)
                return Unauthorized(new { message = "请先登录" });

            var summary = await _service.GetReaderFineSummaryAsync(readerId.Value);
            return Ok(summary);
        }

        #endregion

        #region 预约管理

        /// <summary>
        /// 预约图书
        /// </summary>
        /// <param name="request">预约请求</param>
        /// <returns>预约结果</returns>
        [HttpPost("reserve")]
        public async Task<IActionResult> ReserveBook([FromBody] ReserveBookRequest request)
        {
            var readerId = GetCurrentReaderId();
            if (readerId == null)
                return Unauthorized(new { message = "请先登录" });

            var result = await _service.ReserveBookAsync(readerId.Value, request);
            return Ok(result);
        }

        /// <summary>
        /// 取消预约
        /// </summary>
        /// <param name="request">取消请求</param>
        /// <returns>取消结果</returns>
        [HttpDelete("reserve")]
        public async Task<IActionResult> CancelReserve([FromBody] CancelReserveRequest request)
        {
            var readerId = GetCurrentReaderId();
            if (readerId == null)
                return Unauthorized(new { message = "请先登录" });

            var result = await _service.CancelReserveAsync(readerId.Value, request.ReserveId, request.CancelReason);
            return Ok(new { message = result });
        }

        /// <summary>
        /// 获取预约列表
        /// </summary>
        /// <returns>预约记录列表</returns>
        [HttpGet("reserves")]
        public async Task<IActionResult> GetReserves()
        {
            var readerId = GetCurrentReaderId();
            if (readerId == null)
                return Unauthorized(new { message = "请先登录" });

            var reserves = await _service.GetReaderReservesAsync(readerId.Value);
            return Ok(reserves);
        }

        #endregion

        #region 荐购管理

        /// <summary>
        /// 荐购图书
        /// </summary>
        /// <param name="request">荐购请求</param>
        /// <returns>荐购结果</returns>
        [HttpPost("recommend")]
        public async Task<IActionResult> RecommendPurchase([FromBody] RecommendPurchaseRequest request)
        {
            var readerId = GetCurrentReaderId();
            if (readerId == null)
                return Unauthorized(new { message = "请先登录" });

            var result = await _service.RecommendPurchaseAsync(readerId.Value, request);
            return Ok(result);
        }

        /// <summary>
        /// 获取荐购列表
        /// </summary>
        /// <returns>荐购记录列表</returns>
        [HttpGet("recommends")]
        public async Task<IActionResult> GetRecommends()
        {
            var readerId = GetCurrentReaderId();
            if (readerId == null)
                return Unauthorized(new { message = "请先登录" });

            var recommends = await _service.GetReaderRecommendsAsync(readerId.Value);
            return Ok(recommends);
        }

        #endregion

        #region 消息通知

        /// <summary>
        /// 获取消息通知列表
        /// </summary>
        /// <param name="isRead">是否已读（Y/N）</param>
        /// <param name="type">通知类型</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageNum">页码</param>
        /// <returns>通知列表</returns>
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] string isRead = null,
            [FromQuery] string type = null,
            [FromQuery] int pageSize = 20,
            [FromQuery] int pageNum = 1)
        {
            var readerId = GetCurrentReaderId();
            if (readerId == null)
                return Unauthorized(new { message = "请先登录" });

            var result = await _service.GetReaderNotificationsAsync(
                readerId.Value, isRead, type, pageSize, pageNum);
            return Ok(result);
        }

        /// <summary>
        /// 获取未读通知数量
        /// </summary>
        /// <returns>未读数量</returns>
        [HttpGet("notifications/unread/count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var readerId = GetCurrentReaderId();
            if (readerId == null)
                return Unauthorized(new { message = "请先登录" });

            var count = await _service.GetUnreadNotificationCountAsync(readerId.Value);
            return Ok(new { unreadCount = count });
        }

        /// <summary>
        /// 标记通知为已读
        /// </summary>
        /// <param name="notificationId">通知ID</param>
        /// <returns>操作结果</returns>
        [HttpPut("notifications/{notificationId}/read")]
        public async Task<IActionResult> MarkNotificationRead(int notificationId)
        {
            var readerId = GetCurrentReaderId();
            if (readerId == null)
                return Unauthorized(new { message = "请先登录" });

            var result = await _service.MarkNotificationReadAsync(readerId.Value, notificationId);
            return Ok(new { success = result });
        }

        /// <summary>
        /// 标记所有通知为已读
        /// </summary>
        /// <returns>已标记数量</returns>
        [HttpPut("notifications/read/all")]
        public async Task<IActionResult> MarkAllNotificationsRead()
        {
            var readerId = GetCurrentReaderId();
            if (readerId == null)
                return Unauthorized(new { message = "请先登录" });

            var count = await _service.MarkAllNotificationsReadAsync(readerId.Value);
            return Ok(new { markedCount = count });
        }

        #endregion
    }
}
