using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using backend.Services.Web;
using backend.Models;

namespace backend.Controllers.ReaderFeature
{
    /// <summary>
    /// WebSocket连接端点
    /// </summary>
    [ApiController]
    [Route("api/ws")]
    public class WebSocketController : ControllerBase
    {
        private readonly WebSocketNotificationService _notificationService;
        private readonly SecurityService _securityService;

        public WebSocketController(WebSocketNotificationService notificationService, SecurityService securityService)
        {
            _notificationService = notificationService;
            _securityService = securityService;
        }

        /// <summary>
        /// WebSocket连接端点 - 客户端通过 /api/ws/connect 连接
        /// </summary>
        [Route("connect")]
        public async Task Get()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            var loginUser = _securityService.GetLoginUser();
            string userId = "";
            if (loginUser != null)
            {
                if (loginUser.User is Reader r) userId = $"reader_{r.ReaderID}";
                else if (loginUser.User is Librarian l) userId = $"librarian_{l.LibrarianID}";
            }

            if (string.IsNullOrEmpty(userId))
            {
                HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await _notificationService.HandleWebSocketAsync(webSocket, userId);
        }
    }
}
