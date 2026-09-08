public class BookShelfRepository
{
    private readonly string _connectionString;

    public BookShelfRepository(string connectionString)
    {
        _connectionString = connectionString;
    }



    public async Task<IEnumerable<BookDto>> SearchBookWhichShelfAsync(string keyword)
    {
        var sql = @"
        SELECT   
               i.TITLE ,
               s.BUILDINGID,
               s.SHELFCODE,
               s.FLOOR,
               s.ZONE,
               b.STATUS,
               b.BOOKID,
               b.BARCODE
        FROM Book b
        JOIN BookInfo i ON b.ISBN = i.ISBN LEFT JOIN BookShelf s ON (b.SHELFID = s.SHELFID )
        WHERE LOWER(i.TITLE) LIKE :keyword";

        using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
        await connection.OpenAsync();

        return await Dapper.SqlMapper.QueryAsync<BookDto>(
            connection, sql, new { keyword = $"%{keyword.ToLower()}%" });
    }

    public async Task<IEnumerable<BookShelf>> SearchShelfAsync(string keyword)
    {
        var sql = @"
        SELECT   
               SHELFID,BUILDINGID,SHELFCODE,FLOOR,ZONE
        FROM BOOKSHELF 
         ";

        using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
        await connection.OpenAsync();

        return await Dapper.SqlMapper.QueryAsync<BookShelf>(
            connection, sql, new { keyword = $" %{keyword.ToLower()}%" });
    }

    public async Task<int> AddShelfAsync(int buildingid,
    string ?shelfcode,
    int floor,
    string ?zone)
    {
        var sql = @"
    INSERT INTO BOOKSHELF (
        BUILDINGID,
        SHELFCODE,
        FLOOR,
        ZONE
    ) VALUES (
        :buildingid,
        :shelfcode,
        :floor,
        :zone
    )";

        using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
        await connection.OpenAsync();

        return await Dapper.SqlMapper.ExecuteAsync(
            connection, sql, new { buildingid, shelfcode, floor, zone });
    }

/*    public async Task<bool> HasBooks(int shelfId)
    {
        const string sql = "SELECT COUNT(1) FROM BOOK WHERE SHELFID = :shelfId";
        using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
       
        
        return await connection.ExecuteScalarAsync<int>(sql, new { shelfId }) > 0;
    }*/

    public async Task<int> DeleteShelfAsync(int shelfId)
    {
        const string sql = "DELETE FROM BOOKSHELF WHERE SHELFID = :shelfId";
      
       
        using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
        await connection.OpenAsync();

        return await Dapper.SqlMapper.ExecuteAsync(
            connection, sql, new { shelfId });
    }

    public async Task<int> CheckShelfHasBooksAsync(int shelfId)
    {
        const string sql = "SELECT COUNT(1) FROM BOOK WHERE SHELFID = :shelfId";
        using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
        await connection.OpenAsync();
        var count=await Dapper.SqlMapper.ExecuteScalarAsync<int>(
            connection, sql, new { shelfId });
        
        return count;
    }

    public async Task<int> FindShelfIdAsync(int buildingId, string shelfCode, int floor, string zone)
    {
        const string sql = @"
        SELECT SHELFID 
        FROM BOOKSHELF 
        WHERE BUILDINGID = :buildingId 
          AND SHELFCODE = :shelfCode 
          AND FLOOR = :floor 
          AND ZONE = :zone
        FETCH FIRST 1 ROWS ONLY";

        using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
        await connection.OpenAsync();

        return await Dapper.SqlMapper.ExecuteScalarAsync<int>(
            connection, sql, new { buildingId, shelfCode, floor, zone });
    }

    public async Task<int> ReturnBookAsync(int bookId, int shelfId)
    {
        const string sql = @"
        UPDATE BOOK 
        SET  SHELFID = :shelfId
        WHERE BOOKID = :bookId";

        using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
        await connection.OpenAsync();

        return await Dapper.SqlMapper.ExecuteAsync(
            connection, sql, new { bookId, shelfId });
    }

    // BookShelfRepository.cs 中添加借出数据库操作
    public async Task<int> BorrowBookAsync(int bookId)
    {
        const string sql = @"
    UPDATE BOOK 
    SET STATUS = '下架', 
        SHELFID = NULL
    WHERE BOOKID = :bookId";

        using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
        await connection.OpenAsync();

        return await Dapper.SqlMapper.ExecuteAsync(
            connection, sql, new { bookId });
    }

    // BookShelfRepository.cs 中添加获取书架书籍方法
    public async Task<IEnumerable<ShelfBookDto>> GetShelfBooksAsync(int shelfId)
    {
        var sql = @"
    SELECT 
        b.BOOKID,
        i.TITLE,
        i.AUTHOR,
        b.STATUS,
        b.BARCODE
    FROM BOOK b
    JOIN BOOKINFO i ON b.ISBN = i.ISBN
    WHERE b.SHELFID = :shelfId
    ORDER BY i.TITLE";

        using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
        await connection.OpenAsync();

        return await Dapper.SqlMapper.QueryAsync<ShelfBookDto>(
            connection, sql, new { shelfId });
    }
}