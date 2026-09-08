using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTOs.Admin;
using backend.Services.Admin;

namespace backend.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/purchase-analysis")]
    public class PurchaseAnalysisController : ControllerBase
    {
        private readonly PurchaseAnalysisService _service;

        public PurchaseAnalysisController(PurchaseAnalysisService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<PurchaseAnalysisDto>> GetAnalysis()
        {
            return Ok(await _service.GetPurchaseAnalysisAsync());
        }

        [HttpGet("logs")]
        public async Task<ActionResult<IEnumerable<PurchaseLogDto>>> GetLogs()
        {
            return Ok(await _service.GetPurchaseLogsAsync());
        }

        [HttpPost("logs")]
        public async Task<IActionResult> AddLog([FromBody] CreatePurchaseLogDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.LogText))
            {
                return BadRequest("Log text cannot be empty.");
            }
            await _service.AddPurchaseLogAsync(dto.LogText);
            return CreatedAtAction(nameof(GetLogs), new { }, null);
        }
    }
}