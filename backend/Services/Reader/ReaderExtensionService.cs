using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Dapper;
using backend.DTOs.ReaderDtos;
using backend.DTOs;   // PagedResult<T>
using Backend.Data;

namespace backend.Services.ReaderService
{
    /// <summary>
    /// 读者扩展服务实现
    /// 包含：续借、罚款管理、预约、荐购、通知等功能
    /// </summary>
    public class ReaderExtensionService : IReaderExtensionService
    {
        private readonly string _connectionString;

        public ReaderExtensionService(string connectionString)
        {
            _connectionString = connectionString;
        }

        private OracleConnection CreateConnection()
        {
            return new OracleConnection(_connectionString);
        }

        /// <summary>
        /// 续借图书
        /// </summary>
        public async Task<RenewBookResponse> RenewBookAsync(int readerId, string bookId)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var resultParam = new OracleParameter("p_result", OracleDbType.Varchar2, 4000)
            {
                Direction = ParameterDirection.Output
            };
            var newDueTimeParam = new OracleParameter("p_new_due_time", OracleDbType.TimeStamp)
            {
                Direction = ParameterDirection.Output
            };

            var parameters = new DynamicParameters();
            parameters.Add("p_reader_id", readerId);
            parameters.Add("p_book_id", bookId);
            parameters.Add("p_result", resultParam, dbType: DbType.Object, direction: ParameterDirection.Output);
            parameters.Add("p_new_due_time", newDueTimeParam, dbType: DbType.Object, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("sp_renew_book", parameters, commandType: CommandType.StoredProcedure);

            var result = resultParam.Value?.ToString();
            DateTime? newDueTime = null;
            if (newDueTimeParam.Value != null && newDueTimeParam.Value != DBNull.Value)
            {
                newDueTime = Convert.ToDateTime(newDueTimeParam.Value);
            }

            return new RenewBookResponse
            {
                Success = result?.StartsWith("SUCCESS") ?? false,
                Message = result?.Replace("SUCCESS: ", "").Replace("ERROR: ", "") ?? "未知错误",
                NewDueTime = newDueTime
            };
        }

        /// <summary>
        /// 获取读者罚款列表
        /// </summary>
        public async Task<List<FineRecordDto>> GetReaderFinesAsync(int readerId)
        {
            using var connection = CreateConnection();
            var result = await connection.QueryAsync<FineRecordDto>(
                @"SELECT
                    f.FineID AS FineId,
                    f.Amount,
                    f.PaidAmount,
                    f.Amount - f.PaidAmount AS UnpaidAmount,
                    f.FineDate AS FineDate,
                    f.PayStatus AS PayStatus,
                    bi.Title AS BookTitle,
                    bi.ISBN,
                    f.Remark
                FROM Fine f
                LEFT JOIN BorrowRecord br ON f.BorrowRecordID = br.BorrowRecordID
                LEFT JOIN Book b ON br.BookID = b.BookID
                LEFT JOIN BookInfo bi ON b.ISBN = bi.ISBN
                WHERE f.ReaderID = :readerId
                ORDER BY f.FineDate DESC",
                new { readerId }
            );
            return result.AsList();
        }

        /// <summary>
        /// 获取读者罚款列表（分页）
        /// </summary>
        public async Task<PagedResult<FineRecordDto>> GetReaderFinesPagedAsync(int readerId, int pageSize, int pageNum, string status = null)
        {
            using var connection = CreateConnection();

            var whereClause = "WHERE f.ReaderID = :readerId";
            if (!string.IsNullOrEmpty(status))
            {
                whereClause += status switch
                {
                    "未缴纳" => " AND f.PayStatus = '未缴纳'",
                    "已缴纳" => " AND f.PayStatus = '已缴纳'",
                    _ => ""
                };
            }

            // 查询总数
            var countSql = $"SELECT COUNT(*) FROM Fine f {whereClause}";
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { readerId });

            // 查询分页数据
            var offset = (pageNum - 1) * pageSize;
            var dataSql = $@"
                SELECT
                    f.FineID AS FineId,
                    f.Amount,
                    f.PaidAmount,
                    f.Amount - f.PaidAmount AS UnpaidAmount,
                    f.FineDate AS FineDate,
                    f.PayStatus AS PayStatus,
                    bi.Title AS BookTitle,
                    bi.ISBN,
                    f.Remark
                FROM Fine f
                LEFT JOIN BorrowRecord br ON f.BorrowRecordID = br.BorrowRecordID
                LEFT JOIN Book b ON br.BookID = b.BookID
                LEFT JOIN BookInfo bi ON b.ISBN = bi.ISBN
                {whereClause}
                ORDER BY f.FineDate DESC
                OFFSET :Offset ROWS FETCH NEXT :PageSize ROWS ONLY";

            var data = (await connection.QueryAsync<FineRecordDto>(dataSql,
                new { readerId, Offset = offset, PageSize = pageSize })).AsList();

            return new PagedResult<FineRecordDto>
            {
                Data = data,
                TotalCount = totalCount,
                PageNum = pageNum,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// 获取读者罚款汇总
        /// </summary>
        public async Task<FineSummaryDto> GetReaderFineSummaryAsync(int readerId)
        {
            using var connection = CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<FineSummaryDto>(
                @"SELECT 
                    COUNT(*) AS TotalFines,
                    SUM(Amount) AS TotalAmount,
                    SUM(PaidAmount) AS UnpaidAmount,
                    SUM(Amount - PaidAmount) AS UnpaidAmount
                FROM Fine
                WHERE ReaderID = :readerId",
                new { readerId }
            );

            if (result == null)
            {
                return new FineSummaryDto();
            }

            // 查询未缴纳数量
            var unpaidCount = await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) FROM Fine 
                WHERE ReaderID = :readerId 
                AND PayStatus IN ('未缴纳', '部分缴纳')",
                new { readerId }
            );
            result.UnpaidFines = unpaidCount;

            return result;
        }

        /// <summary>
        /// 预约图书
        /// </summary>
        public async Task<ReserveBookResponse> ReserveBookAsync(int readerId, ReserveBookRequest request)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var resultParam = new OracleParameter("p_result", OracleDbType.Varchar2, 4000)
            {
                Direction = ParameterDirection.Output
            };
            var reserveIdParam = new OracleParameter("p_reserve_id", OracleDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };

            await connection.ExecuteAsync("sp_reserve_book", new
            {
                p_reader_id = readerId,
                p_isbn = request.ISBN,
                p_expected_days = request.ExpectedDays,
                p_result = resultParam,
                p_reserve_id = reserveIdParam
            }, commandType: CommandType.StoredProcedure);

            var result = resultParam.Value?.ToString();
            int? reserveId = null;
            if (reserveIdParam.Value != null && reserveIdParam.Value != DBNull.Value)
            {
                reserveId = Convert.ToInt32(reserveIdParam.Value);
            }

            return new ReserveBookResponse
            {
                Success = result?.StartsWith("SUCCESS") ?? false,
                Message = result?.Replace("SUCCESS: ", "").Replace("HINT: ", "").Replace("ERROR: ", "") ?? "未知错误",
                ReserveId = reserveId
            };
        }

        /// <summary>
        /// 取消预约
        /// </summary>
        public async Task<string> CancelReserveAsync(int readerId, int reserveId, string reason)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var resultParam = new OracleParameter("p_result", OracleDbType.Varchar2, 4000)
            {
                Direction = ParameterDirection.Output
            };

            await connection.ExecuteAsync("sp_cancel_reserve", new
            {
                p_reserve_id = reserveId,
                p_reader_id = readerId,
                p_cancel_reason = reason,
                p_result = resultParam
            }, commandType: CommandType.StoredProcedure);

            return resultParam.Value?.ToString()?.Replace("SUCCESS: ", "").Replace("ERROR: ", "") ?? "未知错误";
        }

        /// <summary>
        /// 获取读者预约列表
        /// </summary>
        public async Task<List<ReserveRecordDto>> GetReaderReservesAsync(int readerId)
        {
            using var connection = CreateConnection();
            var result = await connection.QueryAsync<ReserveRecordDto>(
                @"SELECT 
                    rb.ReserveID AS ReserveId,
                    rb.ISBN,
                    bi.Title AS BookTitle,
                    bi.Author,
                    rb.ReserveTime,
                    rb.ExpectedBorrowTime,
                    rb.ExpireTime,
                    rb.Status,
                    rb.Remark
                FROM ReserveBook rb
                JOIN BookInfo bi ON rb.ISBN = bi.ISBN
                WHERE rb.ReaderID = :readerId
                ORDER BY rb.ReserveTime DESC",
                new { readerId }
            );
            return result.AsList();
        }

        /// <summary>
        /// 荐购图书
        /// </summary>
        public async Task<RecommendPurchaseResponse> RecommendPurchaseAsync(int readerId, RecommendPurchaseRequest request)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var resultParam = new OracleParameter("p_result", OracleDbType.Varchar2, 4000)
            {
                Direction = ParameterDirection.Output
            };
            var recommendIdParam = new OracleParameter("p_recommend_id", OracleDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };

            await connection.ExecuteAsync("sp_recommend_purchase", new
            {
                p_reader_id = readerId,
                p_isbn = request.ISBN ?? (object)DBNull.Value,
                p_title = request.Title,
                p_author = request.Author ?? (object)DBNull.Value,
                p_publisher = request.Publisher ?? (object)DBNull.Value,
                p_publish_year = request.PublishYear,
                p_reason = request.Reason ?? (object)DBNull.Value,
                p_result = resultParam,
                p_recommend_id = recommendIdParam
            }, commandType: CommandType.StoredProcedure);

            var result = resultParam.Value?.ToString();
            int? recommendId = null;
            if (recommendIdParam.Value != null && recommendIdParam.Value != DBNull.Value)
            {
                recommendId = Convert.ToInt32(recommendIdParam.Value);
            }

            return new RecommendPurchaseResponse
            {
                Success = result?.StartsWith("SUCCESS") ?? false,
                Message = result?.Replace("SUCCESS: ", "").Replace("ERROR: ", "") ?? "未知错误",
                RecommendId = recommendId
            };
        }

        /// <summary>
        /// 获取读者荐购列表
        /// </summary>
        public async Task<List<RecommendPurchaseDto>> GetReaderRecommendsAsync(int readerId)
        {
            using var connection = CreateConnection();
            var result = await connection.QueryAsync<RecommendPurchaseDto>(
                @"SELECT 
                    rp.RecommendID AS RecommendId,
                    rp.ISBN,
                    rp.Title,
                    rp.Author,
                    rp.Publisher,
                    rp.PublishYear AS PublishYear,
                    rp.Reason,
                    rp.RecommendTime,
                    rp.Status,
                    rp.HandleTime,
                    rp.HandleResult
                FROM RecommendPurchase rp
                WHERE rp.ReaderID = :readerId
                ORDER BY rp.RecommendTime DESC",
                new { readerId }
            );
            return result.AsList();
        }

        /// <summary>
        /// 获取读者消息通知
        /// </summary>
        public async Task<NotificationListResponse> GetReaderNotificationsAsync(
            int readerId, string isRead = null, string type = null, 
            int pageSize = 20, int pageNum = 1)
        {
            using var connection = CreateConnection();

            var whereClause = "WHERE ReaderID = :readerId AND Status = '有效'";
            if (!string.IsNullOrEmpty(isRead))
            {
                whereClause += " AND IsRead = :isRead";
            }
            if (!string.IsNullOrEmpty(type))
            {
                whereClause += " AND Type = :type";
            }

            var offset = (pageNum - 1) * pageSize;

            var notifications = await connection.QueryAsync<NotificationDto>(
                $@"SELECT 
                    NotificationID AS NotificationId,
                    Title,
                    Content,
                    Type,
                    Priority,
                    CreateTime,
                    IsRead,
                    ReadTime,
                    RelatedType,
                    RelatedID
                FROM Notification
                {whereClause}
                ORDER BY CreateTime DESC
                OFFSET :offset ROWS FETCH NEXT :pageSize ROWS ONLY",
                new { readerId, isRead, type, offset, pageSize }
            );

            var totalCount = await connection.ExecuteScalarAsync<int>(
                $@"SELECT COUNT(*) FROM Notification {whereClause}",
                new { readerId, isRead, type }
            );

            var unreadCount = await GetUnreadNotificationCountAsync(readerId);

            return new NotificationListResponse
            {
                Notifications = notifications.AsList(),
                TotalCount = totalCount,
                UnreadCount = unreadCount
            };
        }

        /// <summary>
        /// 获取未读通知数量
        /// </summary>
        public async Task<int> GetUnreadNotificationCountAsync(int readerId)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) FROM Notification 
                WHERE ReaderID = :readerId 
                AND IsRead = 'N' 
                AND Status = '有效'",
                new { readerId }
            );
        }

        /// <summary>
        /// 标记通知为已读
        /// </summary>
        public async Task<bool> MarkNotificationReadAsync(int readerId, int notificationId)
        {
            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(
                @"UPDATE Notification 
                SET IsRead = 'Y', ReadTime = SYSTIMESTAMP 
                WHERE NotificationID = :notificationId AND ReaderID = :readerId",
                new { notificationId, readerId }
            );
            return rows > 0;
        }

        /// <summary>
        /// 标记所有通知为已读
        /// </summary>
        public async Task<int> MarkAllNotificationsReadAsync(int readerId)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteAsync(
                @"UPDATE Notification 
                SET IsRead = 'Y', ReadTime = SYSTIMESTAMP 
                WHERE ReaderID = :readerId AND IsRead = 'N' AND Status = '有效'",
                new { readerId }
            );
        }
    }
}
