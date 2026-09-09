using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTOs.Admin;

namespace backend.Repositories.Admin
{
    public class DashboardRepository
    {
        private readonly string _connectionString;

        public DashboardRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> ScalarIntAsync(string sql)
        {
            using var conn = new OracleConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql);
        }

        public async Task<decimal> ScalarDecimalAsync(string sql)
        {
            using var conn = new OracleConnection(_connectionString);
            var value = await conn.ExecuteScalarAsync<decimal?>(sql);
            return value ?? 0;
        }

        public async Task<IEnumerable<DashboardTrendItemDto>> GetTrendAsync()
        {
            var sql = @"SELECT TO_CHAR(TRUNC(BORROWTIME),'MM-DD') AS ""Date"", COUNT(*) AS ""Count""
                        FROM BORROWRECORD
                        WHERE BORROWTIME >= TRUNC(SYSDATE) - 6
                        GROUP BY TRUNC(BORROWTIME)
                        ORDER BY TRUNC(BORROWTIME)";
            using var conn = new OracleConnection(_connectionString);
            return await conn.QueryAsync<DashboardTrendItemDto>(sql);
        }

        public async Task<IEnumerable<DashboardHotBookDto>> GetHotBooksAsync()
        {
            var sql = @"SELECT bi.TITLE AS Title, bi.AUTHOR AS Author, COUNT(*) AS ""BorrowCount""
                        FROM BORROWRECORD br
                        JOIN BOOK b ON br.BOOKID = b.BOOKID
                        JOIN BOOKINFO bi ON b.ISBN = bi.ISBN
                        GROUP BY bi.TITLE, bi.AUTHOR
                        ORDER BY COUNT(*) DESC
                        FETCH FIRST 5 ROWS ONLY";
            using var conn = new OracleConnection(_connectionString);
            return await conn.QueryAsync<DashboardHotBookDto>(sql);
        }
    }
}
