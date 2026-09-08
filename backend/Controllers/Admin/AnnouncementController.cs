using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTOs.Admin;
using backend.Services.Admin;

namespace backend.Controllers.Admin
{
    [ApiController]
    public class AnnouncementController : ControllerBase
    {
        private readonly AnnouncementService _service;

        public AnnouncementController(AnnouncementService service)
        {
            _service = service;
        }

        // Public endpoint for homepage
        [HttpGet("api/announcements/public")]
        public async Task<ActionResult<PublicAnnouncementsDto>> GetPublicAnnouncements()
        {
            return Ok(await _service.GetPublicAnnouncementsAsync());
        }

        // Admin endpoints
        [HttpGet("api/admin/announcements")]
        public async Task<ActionResult<IEnumerable<AnnouncementDto>>> GetAllAnnouncements()
        {
            return Ok(await _service.GetAllAnnouncementsAsync());
        }

        [HttpPost("api/admin/announcements")]
        public async Task<ActionResult<AnnouncementDto>> CreateAnnouncement([FromBody] UpsertAnnouncementDto dto)
        {
            var adminId = 8; // Placeholder for logged-in admin's ID
            var created = await _service.CreateAnnouncementAsync(dto, adminId);
            return CreatedAtAction(nameof(GetAllAnnouncements), new { id = created.AnnouncementID }, created);
        }

        [HttpPut("api/admin/announcements/{id}")]
        public async Task<ActionResult<AnnouncementDto>> UpdateAnnouncement(int id, [FromBody] UpsertAnnouncementDto dto)
        {
            var updated = await _service.UpdateAnnouncementAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpPut("api/admin/announcements/{id}/takedown")]
        public async Task<IActionResult> TakedownAnnouncement(int id)
        {
            var success = await _service.TakedownAnnouncementAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}