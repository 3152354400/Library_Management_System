using Dapper;
using Oracle.ManagedDataAccess.Client;
using backend.Models;
using backend.DTOs;

namespace backend.Repositories.BorrowRecordRepository;

public class BorrowRecordRepository
{
    private readonly string _connectionString;

    public BorrowRecordRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<BorrowRecordDetailDto?> GetByIDAsync(int borrowRecordID)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT *
            FROM V_ReaderBorrowDetail
            WHERE BorrowRecordID = :BorrowRecordID";

        return await connection.QueryFirstOrDefaultAsync<BorrowRecordDetailDto>(sql, new { BorrowRecordID = borrowRecordID });
    }

    public async Task<IEnumerable<BorrowRecordDetailDto>> GetByReaderIDAsync(string readerID)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT *
            FROM V_ReaderBorrowDetail
            WHERE ReaderID = :ReaderID
            ORDER BY BorrowTime DESC";

        return await connection.QueryAsync<BorrowRecordDetailDto>(sql, new { ReaderID = int.Parse(readerID) });
    }

    public async Task<IEnumerable<BorrowRecordDetailDto>> GetByBookIDAsync(string bookID)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT *
            FROM V_ReaderBorrowDetail
            WHERE BookID = :BookID
            ORDER BY BorrowTime DESC";

        return await connection.QueryAsync<BorrowRecordDetailDto>(sql, new { BookID = int.Parse(bookID) });
    }

    public async Task<IEnumerable<BorrowRecordDetailDto>> GetAllBorrowRecordsAsync()
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT *
            FROM V_ReaderBorrowDetail
            ORDER BY BorrowTime DESC";

        return await connection.QueryAsync<BorrowRecordDetailDto>(sql);
    }

    public async Task<int> AddAsync(BorrowRecord borrowRecord)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO BorrowRecord (
                ReaderID,
                BookID,
                BorrowTime,
                DueTime,
                ReturnTime,
                OverdueFine,
                Status
            ) VALUES (
                :ReaderID,
                :BookID,
                :BorrowTime,
                :DueTime,
                :ReturnTime,
                :OverdueFine,
                :Status
            )";

        return await connection.ExecuteAsync(sql, borrowRecord);
    }

    public async Task<int> UpdateAsync(BorrowRecord borrowRecord)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            UPDATE BorrowRecord
            SET ReaderID = :ReaderID,
                BookID = :BookID,
                BorrowTime = :BorrowTime,
                DueTime = :DueTime,
                ReturnTime = :ReturnTime,
                OverdueFine = :OverdueFine,
                Status = :Status
            WHERE BorrowRecordID = :BorrowRecordID";

        return await connection.ExecuteAsync(sql, borrowRecord);
    }

    public async Task<int> BorrowBookAsync(string readerId, string bookId)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.ExecuteAsync(
            "BEGIN borrow_book(:ReaderID, :BookID); END;",
            new { ReaderID = int.Parse(readerId), BookID = int.Parse(bookId) }
        );
    }

    public async Task<int> ReturnBookAsync(string readerId, string bookId)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        const string findSql = @"
            SELECT BorrowRecordID
            FROM BorrowRecord
            WHERE ReaderID = :ReaderID
              AND BookID = :BookID
              AND Status IN ('借阅中', '逾期')
            ORDER BY BorrowTime DESC
            FETCH FIRST 1 ROW ONLY";

        var borrowRecordID = await connection.QueryFirstOrDefaultAsync<int?>(
            findSql,
            new { ReaderID = int.Parse(readerId), BookID = int.Parse(bookId) }
        );

        if (borrowRecordID == null)
        {
            var existingCount = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM BorrowRecord WHERE ReaderID = :ReaderID AND BookID = :BookID",
                new { ReaderID = int.Parse(readerId), BookID = int.Parse(bookId) }
            );

            return existingCount > 0 ? 0 : -1;
        }

        await connection.ExecuteAsync(
            "BEGIN return_book(:BorrowRecordID); END;",
            new { BorrowRecordID = borrowRecordID.Value }
        );

        return 1;
    }

    public async Task<int> DeleteAsync(string borrowRecordID)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = "DELETE FROM BorrowRecord WHERE BorrowRecordID = :BorrowRecordID";
        return await connection.ExecuteAsync(sql, new { BorrowRecordID = int.Parse(borrowRecordID) });
    }

    public async Task<BorrowRecord?> GetByReaderAndBookAsync(string readerId, string bookId)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT *
            FROM BorrowRecord
            WHERE ReaderID = :ReaderID
              AND BookID = :BookID
            ORDER BY BorrowTime DESC
            FETCH FIRST 1 ROW ONLY";

        return await connection.QueryFirstOrDefaultAsync<BorrowRecord>(
            sql,
            new { ReaderID = int.Parse(readerId), BookID = int.Parse(bookId) }
        );
    }

    public async Task<List<MyBorrowRecordDto>> GetMyBorrowRecordDtosByReaderIdAsync(string readerId)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT
                ISBN,
                Title AS BookTitle,
                Author AS BookAuthor,
                BorrowTime,
                DueTime,
                ReturnTime,
                OverdueFine,
                BorrowStatus
            FROM V_ReaderBorrowDetail
            WHERE ReaderID = :ReaderID
            ORDER BY BorrowTime DESC";

        return (await connection.QueryAsync<MyBorrowRecordDto>(
            sql,
            new { ReaderID = int.Parse(readerId) }
        )).AsList();
    }

    public async Task<int> GetUnreturnedCountByReaderAsync(string readerId)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT COUNT(*)
            FROM BorrowRecord
            WHERE ReaderID = :ReaderID
              AND Status IN ('借阅中', '逾期')";

        return await connection.ExecuteScalarAsync<int>(sql, new { ReaderID = int.Parse(readerId) });
    }

    public async Task<int> GetOverdueUnreturnedCountByReaderAsync(string readerId)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT COUNT(*)
            FROM BorrowRecord
            WHERE ReaderID = :ReaderID
              AND Status IN ('借阅中', '逾期')
              AND DueTime < CURRENT_TIMESTAMP";

        return await connection.ExecuteScalarAsync<int>(sql, new { ReaderID = int.Parse(readerId) });
    }

    public async Task<int> GetAllOverdueCountByReaderAsync(string readerId)
    {
        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT COUNT(*)
            FROM BorrowRecord
            WHERE ReaderID = :ReaderID
              AND (
                    Status = '逾期'
                    OR (ReturnTime IS NOT NULL AND ReturnTime > DueTime)
                  )";

        return await connection.ExecuteScalarAsync<int>(sql, new { ReaderID = int.Parse(readerId) });
    }
}
