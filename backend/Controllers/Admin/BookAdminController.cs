using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using backend.DTOs.Admin;
using backend.Services.Admin;
using System;

namespace backend.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/books")]
    public class BookAdminController : ControllerBase
    {
        private readonly BookAdminService _service;

        public BookAdminController(BookAdminService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks([FromQuery] string search = "")
        {
            var books = await _service.GetBooksAsync(search);
            return Ok(books);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookDto dto)
        {
            try
            {
                await _service.CreateBookAsync(dto);
                return Ok(new { message = "Book and its copies created successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{isbn}")]
        public async Task<IActionResult> UpdateBook(string isbn, [FromBody] UpdateBookDto dto)
        {
            var success = await _service.UpdateBookInfoAsync(isbn, dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{isbn}")]
        public async Task<IActionResult> TakedownBook(string isbn)
        {
            var success = await _service.TakedownBookAsync(isbn);
            if (!success) return NotFound("No available copies found to takedown for this ISBN.");
            return NoContent();
        }

        [HttpPost("copies")]
        public async Task<IActionResult> AddCopies([FromBody] AddCopiesDto dto)
        {
            try
            {
                await _service.AddCopiesAsync(dto);
                return Ok(new { message = "新副本已成功入库。" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}