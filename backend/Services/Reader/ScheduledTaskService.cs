using System.Data;
using Dapper;
using backend.Services.Web;

namespace backend.Services.ReaderFeature
{
    /// <summary>
    /// 定时任务后台服务
    /// 处理过期预约、发送提醒等后台任务
    /// </summary>
    public class ScheduledTaskService : BackgroundService
    {
        private readonly ILogger<ScheduledTaskService> _logger;
        private readonly IConfiguration _configuration;
        private readonly WebSocketNotificationService _wsService;

        public ScheduledTaskService(
            ILogger<ScheduledTaskService> logger,
            IConfiguration configuration,
            WebSocketNotificationService wsService)
        {
            _logger = logger;
            _configuration = configuration;
            _wsService = wsService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("后台定时任务服务已启动");

            while (!stoppingToken.IsCancellationRequested)
            {
                // 各任务独立容错：预约相关表不在 15 表方案中时优雅跳过，
                // 不影响逾期提醒等核心任务
                try
                {
                    await ProcessExpiredReservations();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "预约过期任务跳过（当前方案无 ReserveBook 表）");
                }

                try
                {
                    await SendReservationReminders();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "预约提醒任务跳过（当前方案无 ReserveBook 表）");
                }

                try
                {
                    await SendOverdueReminders();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "逾期提醒任务跳过（当前方案缺 Notification 表）");
                }

                // 每小时执行一次
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        /// <summary>
        /// 处理过期预约
        /// </summary>
        private async Task ProcessExpiredReservations()
        {
            using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_configuration.GetConnectionString("OracleDB"));
            await connection.OpenAsync();

            var expiredCount = await connection.ExecuteAsync(@"
                UPDATE ReserveBook
                SET Status = '已过期'
                WHERE Status = '已通知'
                  AND ExpireTime < SYSTIMESTAMP");

            if (expiredCount > 0)
            {
                _logger.LogInformation($"处理过期预约: {expiredCount} 条");
            }
        }

        /// <summary>
        /// 发送预约到期提醒
        /// </summary>
        private async Task SendReservationReminders()
        {
            using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_configuration.GetConnectionString("OracleDB"));
            await connection.OpenAsync();

            var reservations = await connection.QueryAsync(@"
                SELECT r.ReserveID, r.ReaderID, r.ISBN, r.ExpireTime,
                       bi.Title AS BookTitle
                FROM ReserveBook r
                JOIN BookInfo bi ON r.ISBN = bi.ISBN
                WHERE r.Status = '已通知'
                  AND r.ExpireTime BETWEEN SYSTIMESTAMP AND SYSTIMESTAMP + INTERVAL '1' DAY
                  AND NOT EXISTS (
                      SELECT 1 FROM Notification n
                      WHERE n.RelatedID = r.ReserveID
                        AND n.Type = '预约到期提醒'
                        AND n.CreateTime > SYSTIMESTAMP - INTERVAL '1' DAY
                  )");

            foreach (var rec in reservations)
            {
                var notifId = await connection.ExecuteScalarAsync<int>(
                    "SELECT NVL(MAX(NotificationID), 0) + 1 FROM Notification");

                await connection.ExecuteAsync(@"
                    INSERT INTO Notification (NotificationID, ReaderID, Title, Content, Type, Priority, IsRead, CreateTime, RelatedID)
                    VALUES (:notifId, :readerId, :title, :content, '预约到期提醒', '重要', 'N', SYSTIMESTAMP, :reserveId)",
                    new
                    {
                        notifId,
                        readerId = rec.ReaderID,
                        title = "预约即将到期",
                        content = $"您预约的《{rec.BOOKTITLE}》将于 {rec.EXPIRETIME:yyyy-MM-dd HH:mm} 到期，请尽快到馆借阅。",
                        reserveId = rec.RESERVEID
                    });

                // 通过WebSocket推送
                await _wsService.NotifyUserAsync($"reader_{rec.READERID}", new
                {
                    type = "notification",
                    title = "预约即将到期",
                    content = $"您预约的《{rec.BOOKTITLE}》即将到期",
                    notificationId = notifId
                });
            }
        }

        /// <summary>
        /// 发送逾期提醒
        /// </summary>
        private async Task SendOverdueReminders()
        {
            using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_configuration.GetConnectionString("OracleDB"));
            await connection.OpenAsync();

            var borrows = await connection.QueryAsync(@"
                SELECT br.BorrowRecordID, br.ReaderID, br.DueTime,
                       bi.Title AS BookTitle
                FROM BorrowRecord br
                JOIN Book b ON br.BookID = b.BookID
                JOIN BookInfo bi ON b.ISBN = bi.ISBN
                WHERE br.ReturnTime IS NULL
                  AND br.DueTime BETWEEN SYSTIMESTAMP AND SYSTIMESTAMP + INTERVAL '3' DAY
                  AND NOT EXISTS (
                      SELECT 1 FROM Notification n
                      WHERE n.RelatedID = br.BorrowRecordID
                        AND n.Type = '逾期提醒'
                        AND n.CreateTime > SYSTIMESTAMP - INTERVAL '3' DAY
                  )");

            foreach (var br in borrows)
            {
                var notifId = await connection.ExecuteScalarAsync<int>(
                    "SELECT NVL(MAX(NotificationID), 0) + 1 FROM Notification");

                await connection.ExecuteAsync(@"
                    INSERT INTO Notification (NotificationID, ReaderID, Title, Content, Type, Priority, IsRead, CreateTime, RelatedID)
                    VALUES (:notifId, :readerId, :title, :content, '逾期提醒', '重要', 'N', SYSTIMESTAMP, :borrowId)",
                    new
                    {
                        notifId,
                        readerId = br.READERID,
                        title = "借阅即将到期",
                        content = $"您借阅的《{br.BOOKTITLE}》将于 {br.DUETIME:yyyy-MM-dd HH:mm} 到期，请及时归还。",
                        borrowId = br.BORROWRECORDID
                    });

                await _wsService.NotifyUserAsync($"reader_{br.READERID}", new
                {
                    type = "notification",
                    title = "借阅即将到期",
                    content = $"您借阅的《{br.BOOKTITLE}》即将到期"
                });
            }
        }
    }
}
