using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using backend.DTOs.Reader;
using backend.Services.Space;
using backend.Services.Web;

namespace backend.Controllers.Space
{
    [ApiController]
    [Route("api/space")]
    public class SpaceController : ControllerBase
    {
        private readonly SpaceService _spaceService;
        private readonly SecurityService _securityService;

        public SpaceController(SpaceService spaceService, SecurityService securityService)
        {
            _spaceService = spaceService;
            _securityService = securityService;
        }

        [HttpGet("seats")]
        public async Task<IActionResult> GetSeatLayout([FromQuery] int buildingId, [FromQuery] int floor)
        {
            var layout = await _spaceService.GetSeatLayoutAsync(buildingId, floor);
            return Ok(layout);
        }

        [HttpPost("reservations/seat")]
        public async Task<IActionResult> CreateSeatReservation([FromBody] CreateSeatReservationDto dto)
        {
            try
            {
                var reader = _securityService.GetReaderFromToken();
                if (reader == null) return Unauthorized();

                await _spaceService.CreateSeatReservationAsync(dto, reader.ReaderID);
                return Ok(new { message = "座位预约成功！" });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, "处理预约时发生未知错误。");
            }
        }

        [HttpGet("my-reservations")]
        public async Task<IActionResult> GetMyReservations()
        {
            var reader = _securityService.GetReaderFromToken();
            if (reader == null) return Unauthorized();
            
            var reservations = await _spaceService.GetMyReservationsAsync(reader.ReaderID);
            return Ok(reservations);
        }
        
        [HttpPut("reservations/{id}/cancel")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var reader = _securityService.GetReaderFromToken();
            if (reader == null) return Unauthorized();

            var success = await _spaceService.CancelReservationAsync(id, reader.ReaderID);
            if (!success) return NotFound("未找到可取消的预约记录。");
            
            return NoContent();
        }
    }
}