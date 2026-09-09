public class BookRepository
{
    private readonly string _connectionString;

    public BookRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    // ========== 按书名/作者搜索（包含分类信息） ==========
    public async Task<IEnumerable<BookInfoDto>> SearchBooksAsync(string keyword)
    {
        Console.WriteLine($"开始搜索图书，关键词: '{keyword}'");
        
        var sql = @"
            SELECT 
                bi.ISBN AS ISBN, 
                bi.Title AS Title, 
                bi.Author AS Author,
                LISTAGG(c.CategoryName, ', ') WITHIN GROUP (ORDER BY c.CategoryName) AS Categories
            FROM BookInfo bi
            LEFT JOIN Book_Classify bc ON bi.ISBN = bc.ISBN
            LEFT JOIN Category c ON bc.CategoryID = c.CategoryID
            WHERE LOWER(bi.Title) LIKE :keyword OR LOWER(bi.Author) LIKE :keyword
            GROUP BY bi.ISBN, bi.Title, bi.Author
            ORDER BY bi.Title";

        using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
        await connection.OpenAsync();
        Console.WriteLine("数据库连接成功");

        var searchPattern = $"%{keyword.ToLower()}%";
        Console.WriteLine($"搜索模式: '{searchPattern}'");

        var books = await Dapper.SqlMapper.QueryAsync<BookInfoDto>(
            connection, sql, new { keyword = searchPattern });

        Console.WriteLine($"找到 {books.Count()} 本图书");

        // 处理null分类信息
        foreach (var book in books)
        {
            if (string.IsNullOrEmpty(book.Categories))
            {
                book.Categories = "暂无分类";
            }
            Console.WriteLine($"图书详细信息:");
            Console.WriteLine($"  ISBN: '{book.ISBN}'");
            Console.WriteLine($"  Title: '{book.Title}'");
            Console.WriteLine($"  Author: '{book.Author}'");
            Console.WriteLine($"  Categories: '{book.Categories}'");
        }

        return books;
    }
    // ========== 按 BookID 状态status流转，原子操作并发安全 ==========
    public async Task<bool> UpdateStatusIfMatchesAsync(int bookId, string expectedStatus, string newStatus, CancellationToken ct = default)
    {
        const string sql = @"
            UPDATE Book
               SET Status = :newStatus
             WHERE BookID = :bookId
               AND Status = :expectedStatus";

        using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
        await connection.OpenAsync(); 

        var affected = await Dapper.SqlMapper.ExecuteAsync(
            connection, sql, new { newStatus, bookId, expectedStatus });

        return affected == 1;
    }
    // ========== 按条码状态status流转（扫码借还用） ==========
    public async Task<bool> UpdateStatusIfMatchesByBarcodeAsync(string barcode, string expectedStatus, string newStatus, CancellationToken ct = default)
    {
        const string sql = @"
            UPDATE Book
               SET Status = :newStatus
             WHERE Barcode = :barcode
               AND Status  = :expectedStatus";

        using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
        await connection.OpenAsync();

        var affected = await Dapper.SqlMapper.ExecuteAsync(
            connection, sql, new { newStatus, barcode, expectedStatus });

        return affected == 1;
    }
    // ========== 按BookID查询,返回 table_Book_Dto ==========
    public async Task<table_Book_Dto?> GetByIdAsync(int bookId, CancellationToken ct = default)
    {
        const string sql = @"SELECT BookID, Barcode, Status, ShelfID, ISBN FROM Book WHERE BookID = :bookId";

        using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
        await connection.OpenAsync();

        return await Dapper.SqlMapper.QueryFirstOrDefaultAsync<table_Book_Dto>(
            connection, sql, new { bookId });
    }

    // ========== 按条码查询,返回 table_Book_Dto ==========
    public async Task<table_Book_Dto?> GetByBarcodeAsync(string barcode, CancellationToken ct = default)
    {
        const string sql = @"SELECT BookID, Barcode, Status, ShelfID, ISBN FROM Book WHERE Barcode = :barcode";

        using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
        await connection.OpenAsync();

        return await Dapper.SqlMapper.QueryFirstOrDefaultAsync<table_Book_Dto>(
            connection, sql, new { barcode });
    }
}