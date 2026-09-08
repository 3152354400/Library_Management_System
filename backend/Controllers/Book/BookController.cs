using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using backend.Services.Web;
using backend.Services.BorrowingService;
using backend.Services.ReaderService;
using backend.DTOs;
using backend.Models;

// ====== 调用方法 ======
// 搜索（书目）：GET /api/Book/search?keyword=...

// 查单册（按ID）：GET /api/Book/{id}

// 查单册（按条码）：GET /api/Book/by-barcode/{barcode}

// 借出（ID）：PATCH /api/Book/{id}/borrow

// 下架（ID）：PATCH /api/Book/{id}/off-shelf

// 还回（ID）：PATCH /api/Book/{id}/return

// 上架（ID）：PATCH /api/Book/{id}/on-shelf

// 借出（条码）：PATCH /api/Book/by-barcode/{barcode}/borrow

// 下架（条码）：PATCH /api/Book/by-barcode/{barcode}/off-shelf

// 还回（条码）：PATCH /api/Book/by-barcode/{barcode}/return

// 上架（条码）：PATCH /api/Book/by-barcode/{barcode}/on- shelf

[ApiController]
[Route("api/[controller]")]
public class BookController : ControllerBase
{
    private readonly BookService _service;
    private readonly BorrowingService _borrowingService;
    private readonly SecurityService _securityService;

    

    public BookController(BookService service, BorrowingService borrowingService, SecurityService securityService)
    {
        _service = service;
        _borrowingService = borrowingService;
        _securityService = securityService;
    }

    // ====== 搜索 ======
    [HttpGet("search")]
    public async Task<IEnumerable<BookInfoDto>> Search(string keyword)
    {
        return await _service.SearchBooksAsync(keyword ?? "");
    }

    // ====== 查询单册（给前端回显用） ======
    [HttpGet("{id:int}")]
    public async Task<ActionResult<table_Book_Dto>> GetById(int id, CancellationToken ct)
    {
        var dto = await _service.GetByIdAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet("by-barcode/{barcode}")]
    public async Task<ActionResult<table_Book_Dto>> GetByBarcode(string barcode, CancellationToken ct)
    {
        var dto = await _service.GetByBarcodeAsync(barcode);
        return dto is null ? NotFound() : Ok(dto);
    }

    // =================================
    // ====== 状态流转：按 BookID ======

    // 正常 -> 借出
    [HttpPatch("{id:int}/borrow")]
    public async Task<IActionResult> Borrow(int id, CancellationToken ct)
    {
        try { await _service.BorrowAsync(id); return NoContent(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    // 正常 -> 下架
    [HttpPatch("{id:int}/off-shelf")]
    public async Task<IActionResult> OffShelf(int id, CancellationToken ct)
    {
        try { await _service.OffShelfAsync(id); return NoContent(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    // 借出 -> 还回(正常)
    [HttpPatch("{id:int}/return")]
    public async Task<IActionResult> Return(int id, CancellationToken ct)
    {
        try { await _service.ReturnAsync(id); return NoContent(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    // 下架 -> 上架(正常)
    [HttpPatch("{id:int}/on-shelf")]
    public async Task<IActionResult> OnShelf(int id, CancellationToken ct)
    {
        try { await _service.OnShelfAsync(id); return NoContent(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    // =================================
    // ====== 状态流转：按条码（扫码场景更方便） ======

    // 正常 -> 借出（按条码）
    [HttpPatch("by-barcode/{barcode}/borrow")]
    public async Task<IActionResult> BorrowByBarcode(string barcode, CancellationToken ct)
    {
        try
        {   
            var dto = await _service.GetByBarcodeAsync(barcode);
            var bookId=dto.BookID.ToString();

            var loginUser = _securityService.GetLoginUser();
            if (!_securityService.CheckIsReader(loginUser))
            {
                return Forbid(); // 或 return Unauthorized();
            }
            var reader = loginUser.User as Reader;
            if (reader == null)
            {
                return BadRequest("当前用户不是读者");
            }
            await _borrowingService.BorrowBookAsync(reader.ReaderID.ToString(),bookId);
            await _service.BorrowByBarcodeAsync(barcode);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // 正常 -> 下架（按条码）
    [HttpPatch("by-barcode/{barcode}/off-shelf")]
    public async Task<IActionResult> OffShelfByBarcode(string barcode, CancellationToken ct)
    {
        try { await _service.OffShelfByBarcodeAsync(barcode); return NoContent(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    // 借出 -> 还回（按条码）
    [HttpPatch("by-barcode/{barcode}/return")]
    public async Task<IActionResult> ReturnByBarcode(string barcode, CancellationToken ct)
    {
        try
        {
            var dto = await _service.GetByBarcodeAsync(barcode);
            var bookId = dto.BookID.ToString();

            var loginUser = _securityService.GetLoginUser();
            if (!_securityService.CheckIsReader(loginUser))
            {
                return Forbid(); // 或 return Unauthorized();
            }
            var reader = loginUser.User as Reader;
            if (reader == null)
            {
                return BadRequest("当前用户不是读者");
            }
            await _borrowingService.ReturnBookAsync(reader.ReaderID.ToString(), bookId);
            await _service.ReturnByBarcodeAsync(barcode);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // 下架 -> 上架(正常)（按条码）
    [HttpPatch("by-barcode/{barcode}/on-shelf")]
    public async Task<IActionResult> OnShelfByBarcode(string barcode, CancellationToken ct)
    {
        try { await _service.OnShelfByBarcodeAsync(barcode); return NoContent(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }
}