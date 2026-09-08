using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("api/[controller]")]
public class BookShelfController : ControllerBase
{
    private readonly BookShelfService _service;

    public BookShelfController(BookShelfService service)
    {
        _service = service;
    }



    // 新增独立搜索功能
    [HttpGet("search_book_which_shelf")]
    public async Task<IEnumerable<BookDto>> SearchBookWhichShelf(
        string keyword)
    {
        return await _service.SearchBookWhichShelfAsync(keyword ?? "");
    }

    // 新增独立搜索功能
    [HttpGet("search_bookshelf")]
    public async Task<IEnumerable<BookShelf>> SearchShelf(
        string keyword)
    {
        return await _service.SearchShelfAsync(keyword ?? "");
    }


    [HttpPost("add_bookshelf")]
    public async Task<IActionResult> AddShelf([FromBody] BookShelfInsertDto dto)
    {
        try
        {
            await _service.AddShelfAsync(dto.BUILDINGID, dto.SHELFCODE, dto.FLOOR, dto.ZONE);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("delete/{shelfId}")]
    public async Task<IActionResult> DeleteShelf(int shelfId)
    {
        try
        {
            var result = await _service.DeleteShelfAsync(shelfId);
            return result > 0 ? Ok() : NotFound("书架不存在");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("has-books/{shelfId:int}")]
    public async Task<IActionResult> CheckShelfHasBooks(int shelfId)
    {
        var count = await _service.CheckShelfHasBooksAsync(shelfId);
        return Ok(count > 0);
    }

    // 检查书架是否存在
    [HttpGet("check-shelf-exists")]
    public async Task<IActionResult> CheckShelfExists(
        [FromQuery] int buildingId,
        [FromQuery] string shelfCode,
        [FromQuery] int floor,
        [FromQuery] string zone)
    {
        var shelfId = await _service.FindShelfIdAsync(buildingId, shelfCode, floor, zone);
        return Ok(shelfId > 0);
    }

    [HttpPost("return-book")]
    public async Task<IActionResult> ReturnBook(
        [FromBody] BookReturnDto dto) // 改为FromBody
    {
        try
        {
            await _service.ReturnBookAsync(dto.BookId, dto.ShelfId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }



    [HttpGet("find-shelf-id")]
    public async Task<IActionResult> FindShelfId(
    [FromQuery] int buildingId,
    [FromQuery] string shelfCode,
    [FromQuery] int floor,
    [FromQuery] string zone)
    {
        var shelfId = await _service.FindShelfIdAsync(buildingId, shelfCode, floor, zone);
        return shelfId > 0 ? Ok(shelfId) : NotFound("书架不存在");
    }

    // BookShelfController.cs 中添加借出API，实际是下架图书
    [HttpPost("borrow-book")]
    public async Task<IActionResult> BorrowBook([FromBody] BookBorrowDto dto)
    {
        try
        {
            await _service.BorrowBookAsync(dto.BookId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // BookShelfController.cs 中添加查看书架书籍API
    [HttpGet("shelf-books/{shelfId:int}")]
    public async Task<IActionResult> GetShelfBooks(int shelfId)
    {
        try
        {
            var books = await _service.GetShelfBooksAsync(shelfId);
            return Ok(books);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
/*
set ASPNETCORE_ENVIRONMENT = Development
dotnet run
Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");/////
*/