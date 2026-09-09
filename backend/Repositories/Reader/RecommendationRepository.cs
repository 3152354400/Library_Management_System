using backend.DTOs;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace backend.Repositories.RecommendationRepository
{
    public class RecommendationRepository
    {
        /**
 * 数据库连接字符串
 */
        private readonly string _connectionString;

        /**
         * 构造函数
         * @param connectionString 数据库连接字符串
         * @return 无
         */
        public RecommendationRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// 获取某个读者的推荐书籍
        /// </summary>
        /// <param name="readerId">读者ID</param>
        /// <param name="topN">推荐书籍数量</param>
        /// <returns>推荐书籍列表</returns>
        public async Task<IEnumerable<RecommendedBookDto>> GetRecommendationAsync(long readerId, int topN = 10)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new OracleCommand("get_recommendations", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            // 函数返回值
            var returnCursor = new OracleParameter("returnCursor", OracleDbType.RefCursor)
            {
                Direction = ParameterDirection.ReturnValue
            };
            cmd.Parameters.Add(returnCursor);

            // 输入参数
            cmd.Parameters.Add("p_ReaderID", OracleDbType.Int32).Value = readerId;
            cmd.Parameters.Add("p_TopN", OracleDbType.Int32).Value = topN;

            await cmd.ExecuteNonQueryAsync();

            using var reader = ((OracleRefCursor)returnCursor.Value).GetDataReader();
            var result = new List<RecommendedBookDto>();

            while (await reader.ReadAsync())
            {
                result.Add(new RecommendedBookDto
                {
                    ISBN = reader["ISBN"]?.ToString(),
                    Title = reader["Title"]?.ToString(),
                    Author = reader["Author"]?.ToString(),
                    BooklistCount = Convert.ToInt32(reader["BooklistCount"])
                });
            }

            return result;

        }

        public async Task<IEnumerable<RecommendedBookDto>> GetRecommendationsAsync(long readerId, int topN = 10)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            // Oracle 随机排序并限制行数
            var sql = @"
        SELECT *
        FROM (
            SELECT ISBN, Title, Author
            FROM BookInfo
            ORDER BY DBMS_RANDOM.VALUE
        )
        WHERE ROWNUM <= :TopN";

            var result = await connection.QueryAsync<RecommendedBookDto>(
                sql,
                new { TopN = topN }
            );

            // BooklistCount 对于随机推荐可置为0
            foreach (var book in result)
            {
                book.BooklistCount = 0;
            }

            return result;
        }

    }

}

