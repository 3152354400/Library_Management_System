public class BookService
{
    private readonly BookRepository _repository;

    public BookService(BookRepository repository)
    {
        _repository = repository;
    }

    // 按书名/作者搜索（BookInfo）
    public Task<IEnumerable<BookInfoDto>> SearchBooksAsync(string keyword)
    {
        return _repository.SearchBooksAsync(keyword);
    }

    // =================================
    // ====== 按 BookID 的状态流转 ======

    // 正常 -> 借出
    public async Task BorrowAsync(int bookId)
    {
        var ok = await _repository.UpdateStatusIfMatchesAsync(
            bookId,
            table_Book_attribute_Status.Normal,
            table_Book_attribute_Status.Borrowed);

        if (!ok) throw new InvalidOperationException("当前状态不可借出（需要为“正常”）。");
    }

    // 正常 -> 下架
    public async Task OffShelfAsync(int bookId)
    {
        var ok = await _repository.UpdateStatusIfMatchesAsync(
            bookId,
            table_Book_attribute_Status.Normal,
            table_Book_attribute_Status.Off);

        if (!ok) throw new InvalidOperationException("当前状态不可下架（需要为“正常”）。");
    }

    // 借出 -> 还回(正常)
    public async Task ReturnAsync(int bookId)
    {
        var ok = await _repository.UpdateStatusIfMatchesAsync(
            bookId,
            table_Book_attribute_Status.Borrowed,
            table_Book_attribute_Status.Normal);

        if (!ok) throw new InvalidOperationException("当前状态不可还回（需要为“借出”）。");
    }

    // 下架 -> 上架(正常)
    public async Task OnShelfAsync(int bookId)
    {
        var ok = await _repository.UpdateStatusIfMatchesAsync(
            bookId,
            table_Book_attribute_Status.Off,
            table_Book_attribute_Status.Normal);

        if (!ok) throw new InvalidOperationException("当前状态不可上架（需要为“下架”）。");
    }

    // =================================
    // ====== 按 barcode 的状态流转 ======

    // 正常 -> 借出（按条码）
    public async Task BorrowByBarcodeAsync(string barcode)
    {
        var ok = await _repository.UpdateStatusIfMatchesByBarcodeAsync(
            barcode,
            table_Book_attribute_Status.Normal,
            table_Book_attribute_Status.Borrowed);

        if (!ok) throw new InvalidOperationException("当前状态不可借出（需要为“正常”）。");
    }

    // 正常 -> 下架（按条码）
    public async Task OffShelfByBarcodeAsync(string barcode)
    {
        var ok = await _repository.UpdateStatusIfMatchesByBarcodeAsync(
            barcode,
            table_Book_attribute_Status.Normal,
            table_Book_attribute_Status.Off);

        if (!ok) throw new InvalidOperationException("当前状态不可下架（需要为“正常”）。");
    }

    // 借出 -> 还回（按条码）
    public async Task ReturnByBarcodeAsync(string barcode)
    {
        var ok = await _repository.UpdateStatusIfMatchesByBarcodeAsync(
            barcode,
            table_Book_attribute_Status.Borrowed,
            table_Book_attribute_Status.Normal);

        if (!ok) throw new InvalidOperationException("当前状态不可还回（需要为“借出”）。");
    }

    // 下架 -> 上架（按条码）
    public async Task OnShelfByBarcodeAsync(string barcode)
    {
        var ok = await _repository.UpdateStatusIfMatchesByBarcodeAsync(
            barcode,
            table_Book_attribute_Status.Off,
            table_Book_attribute_Status.Normal);

        if (!ok) throw new InvalidOperationException("当前状态不可上架（需要为“下架”）。");
    }

    // ====== 回查当前册信息（直接读 Book 表，不用视图） ======

    public Task<table_Book_Dto?> GetByIdAsync(int bookId)
        => _repository.GetByIdAsync(bookId);

    public Task<table_Book_Dto?> GetByBarcodeAsync(string barcode)
        => _repository.GetByBarcodeAsync(barcode);
}