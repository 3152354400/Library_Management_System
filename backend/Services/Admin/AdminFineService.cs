using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Dapper;
using backend.DTOs.ReaderDtos;
using Backend.Data;

namespace backend.Services.Admin
{
    /// <summary>
    /// 管理员罚单管理服务
    /// </summary>
    public class AdminFineService
    {
        private readonly string _connectionString;

        public AdminFineService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// 获取所有待缴罚单
        /// </summary>
        public async Task<List<FineRecordDto>> GetAllPendingFinesAsync()
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();
            var result = await connection.QueryAsync<FineRecordDto>(
                @"SELECT 
                    f.FineID AS FineId,
                    f.FineType AS FineType,
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
                WHERE f.PayStatus IN ('未缴纳', '部分缴纳')
                ORDER BY f.FineDate DESC"
            );
            return result.AsList();
        }

        /// <summary>
        /// 缴纳罚款
        /// </summary>
        public async Task<string> PayFineAsync(int fineId, decimal payAmount, string payMethod, int operatorId)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            var resultParam = new OracleParameter("p_result", OracleDbType.Varchar2, 4000)
            {
                Direction = ParameterDirection.Output
            };

            await connection.ExecuteAsync("sp_pay_fine", new
            {
                p_fine_id = fineId,
                p_pay_amount = payAmount,
                p_pay_method = payMethod,
                p_operator_id = operatorId,
                p_result = resultParam
            }, commandType: CommandType.StoredProcedure);

            var result = resultParam.Value?.ToString() ?? "";
            return result.Replace("SUCCESS: ", "").Replace("ERROR: ", "");
        }

        /// <summary>
        /// 减免罚款
        /// </summary>
        public async Task<bool> WaiveFineAsync(int fineId, int operatorId, string reason)
        {
            using var connection = new OracleConnection(_connectionString);
            var rows = await connection.ExecuteAsync(
                @"UPDATE Fine 
                SET PayStatus = '已减免', 
                    HandlerID = :operatorId,
                    Remark = CONCAT(NVL(Remark, ''), :reason)
                WHERE FineID = :fineId AND PayStatus IN ('未缴纳', '部分缴纳')",
                new { fineId, operatorId, reason = " | 减免说明：" + reason }
            );
            return rows > 0;
        }
    }

    /// <summary>
    /// 管理员荐购处理服务
    /// </summary>
    public class AdminRecommendService
    {
        private readonly string _connectionString;

        public AdminRecommendService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// 获取待处理荐购
        /// </summary>
        public async Task<List<RecommendPurchaseDto>> GetPendingRecommendsAsync()
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();
            var result = await connection.QueryAsync<RecommendPurchaseDto>(
                @"SELECT 
                    RecommendID AS RecommendId,
                    ISBN,
                    Title,
                    Author,
                    Publisher,
                    PublishYear AS PublishYear,
                    Reason,
                    RecommendTime,
                    Status,
                    HandleTime,
                    HandleResult
                FROM RecommendPurchase 
                WHERE Status = '待审核'
                ORDER BY RecommendTime ASC"
            );
            return result.AsList();
        }

        /// <summary>
        /// 处理荐购
        /// </summary>
        public async Task<string> HandleRecommendAsync(int recommendId, int handlerId, string action, string handleResult, decimal? purchasePrice)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            var resultParam = new OracleParameter("p_result", OracleDbType.Varchar2, 4000)
            {
                Direction = ParameterDirection.Output
            };

            await connection.ExecuteAsync("sp_handle_recommend", new
            {
                p_recommend_id = recommendId,
                p_handler_id = handlerId,
                p_action = action,
                p_handle_result = handleResult ?? (object)DBNull.Value,
                p_purchase_price = purchasePrice,
                p_result = resultParam
            }, commandType: CommandType.StoredProcedure);

            var result = resultParam.Value?.ToString() ?? "";
            return result.Replace("SUCCESS: ", "").Replace("ERROR: ", "");
        }
    }
}
