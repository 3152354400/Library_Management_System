using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace backend.Services.Web
{
    /// <summary>
    /// WebSocket通知服务 - 实时推送消息给读者
    /// </summary>
    public class WebSocketNotificationService
    {
        private static readonly ConcurrentDictionary<string, WebSocket> _connections = new();
        private readonly ILogger<WebSocketNotificationService> _logger;

        public WebSocketNotificationService(ILogger<WebSocketNotificationService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 注册新的WebSocket连接
        /// </summary>
        public async Task HandleWebSocketAsync(WebSocket webSocket, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("尝试注册WebSocket连接但userId为空");
                webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "用户未登录", CancellationToken.None);
                return;
            }

            _connections[userId] = webSocket;
            _logger.LogInformation($"WebSocket连接已注册: UserId={userId}, 总连接数={_connections.Count}");

            try
            {
                var buffer = new byte[1024 * 4];
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"WebSocket接收异常: UserId={userId}");
            }
            finally
            {
                _connections.TryRemove(userId, out _);
                _logger.LogInformation($"WebSocket连接已断开: UserId={userId}, 剩余连接数={_connections.Count}");
            }
        }

        /// <summary>
        /// 推送通知给指定用户
        /// </summary>
        public async Task NotifyUserAsync(string userId, object notification)
        {
            if (!_connections.TryGetValue(userId, out var webSocket))
            {
                _logger.LogDebug($"用户 {userId} 未连接WebSocket");
                return;
            }

            if (webSocket.State != WebSocketState.Open)
            {
                _connections.TryRemove(userId, out _);
                return;
            }

            try
            {
                var json = JsonSerializer.Serialize(notification);
                var bytes = Encoding.UTF8.GetBytes(json);
                await webSocket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
                _logger.LogInformation($"通知已推送: UserId={userId}, Content={json}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"WebSocket推送失败: UserId={userId}");
                _connections.TryRemove(userId, out _);
            }
        }

        /// <summary>
        /// 广播通知给所有连接
        /// </summary>
        public async Task BroadcastAsync(object notification)
        {
            var tasks = _connections
                .Where(kvp => kvp.Value.State == WebSocketState.Open)
                .Select(kvp => NotifyUserAsync(kvp.Key, notification));

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 获取当前连接数
        /// </summary>
        public int GetConnectionCount() => _connections.Count;
    }
}
