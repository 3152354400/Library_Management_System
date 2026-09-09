using Microsoft.AspNetCore.Mvc;
using backend.Services.Web;
using backend.Models;
using System.Data;
using Dapper;

namespace backend.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/book-loss")]
    public class BookLossController : ControllerBase
    {
        private readonly SecurityService _securityService;
        private readonly string _connectionString;

        public BookLossController(SecurityService securityService, IConfiguration configuration)
        {
            _securityService = securityService;
            _connectionString = configuration.GetConnectionString("OracleDB") ?? "";
        }

        /// <summary>
        /// 获取所有遗失/损坏报告
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetLossReports([FromQuery] string status = null)
        {
            using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                SELECT 
                    r.ReportID, r.ReaderID, rd.FullName AS ReaderName,
                    r.BookID, b.ISBN, bi.Title AS BookTitle,
                    r.ReportType, r.ReportTime, r.Description,
                    r.EstimatedValue, r.CompensationAmount, r.Status,
                    r.HandleTime, r.HandlerID, r.HandleResult,
                    r.PaymentStatus, r.PaymentTime, r.Remark
                FROM BookLossReport r
                JOIN Reader rd ON r.ReaderID = rd.ReaderID
                JOIN Book b ON r.BookID = b.BookID
                JOIN BookInfo bi ON b.ISBN = bi.ISBN";

            if (!string.IsNullOrEmpty(status))
            {
                sql += " WHERE r.Status = :status";
            }
            sql += " ORDER BY r.ReportTime DESC";

            var reports = await connection.QueryAsync(sql, new { status });
            return Ok(reports);
        }

        /// <summary>
        /// 读者提交遗失/损坏报告
        /// </summary>
        [HttpPost("report")]
        public async Task<IActionResult> SubmitReport([FromBody] BookLossReportDto dto)
        {
            var loginUser = _securityService.GetLoginUser();
            if (!_securityService.CheckIsReader(loginUser))
                return Unauthorized(new { message = "请以读者身份登录" });

            var reader = loginUser.User as Reader;

            using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
            await connection.OpenAsync();

            var reportId = await connection.ExecuteScalarAsync<int>(
                "SELECT NVL(MAX(ReportID), 0) + 1 FROM BookLossReport");

            await connection.ExecuteAsync(@"
                INSERT INTO BookLossReport 
                (ReportID, ReaderID, BookID, ReportType, ReportTime, Description, EstimatedValue, Status)
                VALUES 
                (:ReportID, :ReaderID, :BookID, :ReportType, SYSTIMESTAMP, :Description, :EstimatedValue, '待处理')",
                new
                {
                    ReportID = reportId,
                    ReaderID = reader.ReaderID,
                    dto.BookID,
                    dto.ReportType,
                    dto.Description,
                    dto.EstimatedValue
                });

            return Ok(new { message = "报告已提交，请等待管理员处理", reportId });
        }

        /// <summary>
        /// 管理员确认报告并计算赔偿金额
        /// </summary>
        [HttpPut("{reportId}/confirm")]
        public async Task<IActionResult> ConfirmReport(int reportId, [FromBody] ConfirmLossDto dto)
        {
            var loginUser = _securityService.GetLoginUser();
            if (!_securityService.CheckIsLibrarian(loginUser))
                return Unauthorized(new { message = "请以管理员身份登录" });

            var librarian = loginUser.User as Librarian;

            using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
            await connection.OpenAsync();

            var rows = await connection.ExecuteAsync(@"
                UPDATE BookLossReport
                SET Status = '已确认',
                    CompensationAmount = :amount,
                    HandleTime = SYSTIMESTAMP,
                    HandlerID = :handlerId,
                    HandleResult = :result
                WHERE ReportID = :reportId AND Status = '待处理'",
                new { dto.Amount, handlerId = librarian.LibrarianID, dto.Result, reportId });

            if (rows == 0)
                return BadRequest(new { message = "报告不存在或已被处理" });

            return Ok(new { message = "已确认赔偿金额" });
        }

        /// <summary>
        /// 管理员确认读者已赔偿
        /// </summary>
        [HttpPut("{reportId}/compensate")]
        public async Task<IActionResult> MarkCompensated(int reportId)
        {
            var loginUser = _securityService.GetLoginUser();
            if (!_securityService.CheckIsLibrarian(loginUser))
                return Unauthorized(new { message = "请以管理员身份登录" });

            var librarian = loginUser.User as Librarian;

            using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
            await connection.OpenAsync();

            var rows = await connection.ExecuteAsync(@"
                UPDATE BookLossReport
                SET Status = '已赔偿',
                    PaymentStatus = '已缴纳',
                    PaymentTime = SYSTIMESTAMP,
                    HandlerID = :handlerId
                WHERE ReportID = :reportId AND Status = '已确认'",
                new { handlerId = librarian.LibrarianID, reportId });

            if (rows == 0)
                return BadRequest(new { message = "报告不存在或未确认" });

            return Ok(new { message = "已标记为已赔偿" });
        }
    }

    public class BookLossReportDto
    {
        public string BookID { get; set; }
        public string ReportType { get; set; }  // 遗失/损坏
        public string Description { get; set; }
        public decimal? EstimatedValue { get; set; }
    }

    public class ConfirmLossDto
    {
        public decimal Amount { get; set; }
        public string Result { get; set; }
    }
}
