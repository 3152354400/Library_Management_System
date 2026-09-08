public class BookShelfService
{
    private readonly BookShelfRepository _repository;

    public BookShelfService(BookShelfRepository repository)
    {
        _repository = repository;
    }



    public Task<IEnumerable<BookDto>> SearchBookWhichShelfAsync(string keyword)
    {
        return _repository.SearchBookWhichShelfAsync(keyword);
    }

    public Task<IEnumerable<BookShelf>> SearchShelfAsync(string keyword)
    {
        return _repository.SearchShelfAsync(keyword);
    }

    public async Task AddShelfAsync(int buildingid, string ?shelfcode, int floor, string? zone)
    {
        // 业务验证
       

        var affectedRows = await _repository.AddShelfAsync(buildingid, shelfcode, floor, zone);
        if (affectedRows == 0) throw new Exception("插入失败");
    }

    public async Task<int> DeleteShelfAsync(int shelfId)
    {

        return await _repository.DeleteShelfAsync(shelfId);
    }

    public async Task<int> CheckShelfHasBooksAsync(int shelfId)
    {

        return await _repository.CheckShelfHasBooksAsync(shelfId);
    }

    public async Task<int> FindShelfIdAsync(int buildingId, string shelfCode, int floor, string zone)
    {
        return await _repository.FindShelfIdAsync(buildingId, shelfCode, floor, zone);
    }

    public async Task ReturnBookAsync(int bookId, int shelfId)
    {
        var affectedRows = await _repository.ReturnBookAsync(bookId, shelfId);
        if (affectedRows == 0) throw new Exception("归还失败");
    }

    // BookShelfService.cs 中添加借出方法
    public async Task BorrowBookAsync(int bookId)
    {
        var affectedRows = await _repository.BorrowBookAsync(bookId);
        if (affectedRows == 0) throw new Exception("借出失败");
    }

    // BookShelfService.cs 中添加获取书架书籍方法
    public async Task<IEnumerable<ShelfBookDto>> GetShelfBooksAsync(int shelfId)
    {
        return await _repository.GetShelfBooksAsync(shelfId);
    }
}