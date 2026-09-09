using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTOs.Admin;

namespace backend.Repositories.Admin
{
    public class ReportRepository
    {
        private readonly string _connectionString;

        public ReportRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<ReportDetailDto>> GetPendingReportsAsync()
        {
            const string sql = @"
                SELECT
                    ReportID,
                    ReportReason,
                    ReportTime,
                    ReportStatus,
                    CommentID,
                    ReviewContent,
                    Rating,
                    CommentTime,
                    CommentStatus,
                    ReporterID,
                    ReporterUsername,
                    ISBN,
                    BookTitle
                FROM V_PendingReport
                ORDER BY ReportTime ASC";

            using var connection = new OracleConnection(_connectionString);
            return await connection.QueryAsync<ReportDetailDto>(sql);
        }

        public async Task<bool> HandleReportTransactionAsync(int reportId, int commenterId, int commentId, string newReportStatus, string? newCommentStatus, bool banUser, int librarianId, string? handleResult = null)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            await connection.ExecuteAsync(
                "BEGIN handle_report(:ReportID, :LibrarianID, :Action, :HandleResult); END;",
                new
                {
                    ReportID = reportId,
                    LibrarianID = librarianId,
                    Action = newReportStatus,
                    HandleResult = handleResult ?? newReportStatus
                }
            );

            if (banUser)
            {
                const string readerSql = @"
                    UPDATE Reader
                    SET AccountStatus = '冻结'
                    WHERE ReaderID = (
                        SELECT cm.ReaderID
                        FROM Comment_Table cm
                        WHERE cm.CommentID = :CommentID
                    )";

                await connection.ExecuteAsync(readerSql, new { CommentID = commentId });
            }

            return true;
        }

        public async Task<int> AddReportAsync(ReportDto report)
        {
            const string sql = @"
                INSERT INTO Report (CommentID, ReaderID, ReportReason, ReportTime, Status, LibrarianID)
                VALUES (:CommentID, :ReaderID, :ReportReason, :ReportTime, :Status, :LibrarianID)";

            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.ExecuteAsync(sql, report);
        }
    }
}
