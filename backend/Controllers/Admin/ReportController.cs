using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using backend.DTOs.Admin;
using backend.Services.Admin;
using System;
using System.Collections.Generic; // 确保引入
using backend.Services.Web;
using backend.DTOs.Reader;
using backend.DTOs.Book;
using backend.Models;

namespace backend.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/reports")]
    public class ReportController : ControllerBase
    {
        private readonly ReportService _service;
        private readonly SecurityService _securityService;

        public ReportController(ReportService service, SecurityService securityService)
        {
            _service = service;
            _securityService = securityService;
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<ReportDetailDto>>> GetPendingReports()
        {
            var reports = await _service.GetPendingReportsAsync();
            return Ok(reports);
        }

        [HttpPut("{reportId}")]
        public async Task<IActionResult> HandleReport(int reportId, [FromBody] HandleReportDto dto)
        {
            try
            {
                // In a real app, adminId would come from JWT claims.
                var adminId = 9; // Placeholder
                var success = await _service.HandleReportAsync(reportId, dto.Action, dto.BanUser, adminId);
                if (success)
                {
                    return NoContent();
                }
                return BadRequest("Failed to handle the report.");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
  
        [HttpPost("add")]
        public async Task<ActionResult> AddReport([FromBody] ReportDto report)
        {
            var loginUser = _securityService.GetLoginUser();
        
            // 检查登录用户是否为 Reader
            if (_securityService.CheckIsReader(loginUser))
            {
                var reader = loginUser.User as Reader;
                report.ReaderID = reader.ReaderID;
            }
            
            var result = await _service.AddReportAsync(report);
            if (result > 0)
            {
                return Ok(new { Message = "举报添加成功" });
            }
            return BadRequest(new { Message = "举报添加失败" });
        }

    }
}