using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTOs.Admin;

namespace backend.Repositories.Admin
{
    public class PurchaseAnalysisRepository
    {
        private readonly string _connectionString;

        public PurchaseAnalysisRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private async Task<IEnumerable<BookRankingDto>> GetTop10RankingsAsync(string viewName)
        {
            // 对于新的视图，我们需要调整一下 SELECT 的字段来匹配 DTO
            var sql = viewName == "V_BookRank_By_InstanceBorrow" 
                ? "SELECT Barcode AS ISBN, Title, Author, MetricValue FROM V_BookRank_By_InstanceBorrow FETCH FIRST 10 ROWS ONLY" // 使用 Barcode 作为唯一标识
                : $"SELECT ISBN, Title, Author, MetricValue FROM {viewName} FETCH FIRST 10 ROWS ONLY";

            using var connection = new OracleConnection(_connectionString);
            return await connection.QueryAsync<BookRankingDto>(sql);
        }

        public Task<IEnumerable<BookRankingDto>> GetTop10ByBorrowCountAsync() => GetTop10RankingsAsync("V_BookRank_By_BorrowCount");
        public Task<IEnumerable<BookRankingDto>> GetTop10ByBorrowDurationAsync() => GetTop10RankingsAsync("V_BookRank_By_BorrowDuration");
        public Task<IEnumerable<BookRankingDto>> GetTop10ByReservationCountAsync() => GetTop10RankingsAsync("V_BookRank_By_InstanceBorrow"); // 指向新视图

        public async Task<IEnumerable<PurchaseLogDto>> GetPurchaseLogsAsync()
        {
            var sql = "SELECT LogID, LogText, LogDate FROM PurchaseLog ORDER BY LogDate DESC";
            using var connection = new OracleConnection(_connectionString);
            return await connection.QueryAsync<PurchaseLogDto>(sql);
        }

        public async Task AddPurchaseLogAsync(string logText)
        {
            var sql = "INSERT INTO PurchaseLog (LogText) VALUES (:LogText)";
            using var connection = new OracleConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { LogText = logText });
        }
    }
}