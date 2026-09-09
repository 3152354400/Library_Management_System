using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOs.Book;
using Dapper;

namespace Backend.Repositories.Book
{
    public class BooklistRepository : IBooklistRepository
    {
        private readonly IOracleConnectionFactory _connectionFactory;

        public BooklistRepository(IOracleConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<BooklistSuccessResponse> AddBookToBooklistAsync(int booklistId, AddBookToBooklistRequest request)
        {
            using var conn = await _connectionFactory.CreateAsync();

            const string sql = @"
                INSERT INTO Booklist_Book (BooklistID, ISBN, Notes)
                VALUES (:BooklistID, :ISBN, :Notes)";

            var affected = await conn.ExecuteAsync(sql, new { BooklistID = booklistId, request.ISBN, request.Notes });
            return new BooklistSuccessResponse { Success = affected };
        }

        public async Task<BooklistSuccessResponse> RemoveBookFromBooklistAsync(int booklistId, string isbn)
        {
            using var conn = await _connectionFactory.CreateAsync();

            const string sql = @"
                DELETE FROM Booklist_Book
                WHERE BooklistID = :BooklistID
                  AND ISBN = :ISBN";

            var affected = await conn.ExecuteAsync(sql, new { BooklistID = booklistId, ISBN = isbn });
            return new BooklistSuccessResponse { Success = affected };
        }

        public async Task<BooklistSuccessResponse> CollectBooklistAsync(int booklistId, int readerId, CollectBooklistRequest request)
        {
            using var conn = await _connectionFactory.CreateAsync();

            const string sql = @"
                INSERT INTO Collect (BooklistID, ReaderID, Notes)
                SELECT :BooklistID, :ReaderID, :Notes
                FROM Booklist
                WHERE BooklistID = :BooklistID
                  AND Status = '公开'";

            var affected = await conn.ExecuteAsync(sql, new { BooklistID = booklistId, ReaderID = readerId, request.Notes });
            return new BooklistSuccessResponse { Success = affected };
        }

        public async Task<BooklistSuccessResponse> CancelCollectBooklistAsync(int booklistId, int readerId)
        {
            using var conn = await _connectionFactory.CreateAsync();

            const string sql = @"
                DELETE FROM Collect
                WHERE BooklistID = :BooklistID
                  AND ReaderID = :ReaderID";

            var affected = await conn.ExecuteAsync(sql, new { BooklistID = booklistId, ReaderID = readerId });
            return new BooklistSuccessResponse { Success = affected };
        }

        public async Task<BooklistSuccessResponse> UpdateCollectNotesAsync(int booklistId, int readerId, UpdateCollectNotesRequest request)
        {
            using var conn = await _connectionFactory.CreateAsync();

            const string sql = @"
                UPDATE Collect
                SET Notes = :NewNotes
                WHERE BooklistID = :BooklistID
                  AND ReaderID = :ReaderID";

            var affected = await conn.ExecuteAsync(sql, new { BooklistID = booklistId, ReaderID = readerId, request.NewNotes });
            return new BooklistSuccessResponse { Success = affected };
        }

        public async Task<CreateBooklistResponse> CreateBooklistAsync(CreateBooklistRequest request, int creatorId)
        {
            using var conn = await _connectionFactory.CreateAsync();

            var listCode = $"LIST{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            const string sql = @"
                INSERT INTO Booklist (ListCode, BooklistName, BooklistIntroduction, CreatorID, Status)
                VALUES (:ListCode, :BooklistName, :BooklistIntroduction, :CreatorID, '公开')
                RETURNING BooklistID INTO :BooklistID";

            var parameters = new DynamicParameters();
            parameters.Add("ListCode", listCode);
            parameters.Add("BooklistName", request.BooklistName);
            parameters.Add("BooklistIntroduction", request.BooklistIntroduction);
            parameters.Add("CreatorID", creatorId);
            parameters.Add("BooklistID", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

            var affected = await conn.ExecuteAsync(sql, parameters);
            return new CreateBooklistResponse { BooklistId = parameters.Get<int>("BooklistID"), Success = affected };
        }

        public async Task<BooklistSuccessResponse> DeleteBooklistAsync(int booklistId, int readerId)
        {
            using var conn = await _connectionFactory.CreateAsync();

            const string sql = @"
                UPDATE Booklist
                SET Status = '删除'
                WHERE BooklistID = :BooklistID
                  AND CreatorID = :ReaderID";

            var affected = await conn.ExecuteAsync(sql, new { BooklistID = booklistId, ReaderID = readerId });
            return new BooklistSuccessResponse { Success = affected };
        }

        public async Task<GetBooklistDetailsResponse?> GetBooklistDetailsAsync(int booklistId)
        {
            using var conn = await _connectionFactory.CreateAsync();

            const string infoSql = @"
                SELECT
                    bl.BooklistID AS BooklistId,
                    bl.ListCode,
                    bl.BooklistName,
                    bl.BooklistIntroduction,
                    bl.CreatorID AS CreatorId,
                    r.Username AS CreatorUsername,
                    r.Nickname AS CreatorNickname
                FROM Booklist bl
                JOIN Reader r ON bl.CreatorID = r.ReaderID
                WHERE bl.BooklistID = :BooklistID
                  AND bl.Status <> '删除'";

            var info = await conn.QueryFirstOrDefaultAsync<BooklistInfoDto>(infoSql, new { BooklistID = booklistId });
            if (info == null) return null;

            const string booksSql = @"
                SELECT
                    bb.ISBN,
                    bb.AddTime,
                    bb.Notes,
                    bi.Title,
                    bi.Author
                FROM Booklist_Book bb
                JOIN BookInfo bi ON bb.ISBN = bi.ISBN
                WHERE bb.BooklistID = :BooklistID
                ORDER BY bb.AddTime DESC";

            var books = (await conn.QueryAsync<BookItemDto>(booksSql, new { BooklistID = booklistId })).AsList();

            return new GetBooklistDetailsResponse
            {
                BooklistInfo = info,
                Books = books
            };
        }

        public async Task<RecommendBooklistsResponse> RecommendBooklistsAsync(int booklistId, int limit = 10)
        {
            using var conn = await _connectionFactory.CreateAsync();

            const string sql = @"
                SELECT
                    BooklistID,
                    ListCode,
                    BooklistName,
                    BooklistIntroduction,
                    CreatorID,
                    CreateTime
                FROM Booklist
                WHERE BooklistID <> :BooklistID
                  AND Status = '公开'
                ORDER BY CreateTime DESC
                FETCH FIRST :Limit ROWS ONLY";

            var result = await conn.QueryAsync<RecommendBooklistDto>(sql, new { BooklistID = booklistId, Limit = limit });
            return new RecommendBooklistsResponse { Items = result?.AsList() ?? new List<RecommendBooklistDto>() };
        }

        public async Task<SearchBooklistsByReaderResponse> SearchBooklistsByReaderAsync(int readerId)
        {
            using var conn = await _connectionFactory.CreateAsync();

            const string createdSql = @"
                SELECT
                    BooklistID AS BooklistId,
                    ListCode,
                    BooklistName,
                    BooklistIntroduction,
                    CreatorID AS CreatorId,
                    CAST(NULL AS TIMESTAMP) AS FavoriteTime
                FROM Booklist
                WHERE CreatorID = :ReaderID
                  AND Status <> '删除'
                ORDER BY CreateTime DESC";

            const string collectedSql = @"
                SELECT
                    bl.BooklistID AS BooklistId,
                    bl.ListCode,
                    bl.BooklistName,
                    bl.BooklistIntroduction,
                    bl.CreatorID AS CreatorId,
                    c.FavoriteTime
                FROM Collect c
                JOIN Booklist bl ON c.BooklistID = bl.BooklistID
                WHERE c.ReaderID = :ReaderID
                  AND bl.Status <> '删除'
                ORDER BY c.FavoriteTime DESC";

            var created = (await conn.QueryAsync<SimpleBooklistDto>(createdSql, new { ReaderID = readerId })).AsList();
            var collected = (await conn.QueryAsync<SimpleBooklistDto>(collectedSql, new { ReaderID = readerId })).AsList();

            return new SearchBooklistsByReaderResponse
            {
                Created = created,
                Collected = collected
            };
        }

        public async Task<BooklistSuccessResponse> UpdateBooklistNameAsync(int booklistId, int readerId, UpdateBooklistNameRequest request)
        {
            using var conn = await _connectionFactory.CreateAsync();

            const string sql = @"
                UPDATE Booklist
                SET BooklistName = :NewName
                WHERE BooklistID = :BooklistID
                  AND CreatorID = :ReaderID
                  AND Status <> '删除'";

            var affected = await conn.ExecuteAsync(sql, new { BooklistID = booklistId, ReaderID = readerId, request.NewName });
            return new BooklistSuccessResponse { Success = affected };
        }

        public async Task<BooklistSuccessResponse> UpdateBooklistIntroAsync(int booklistId, int readerId, UpdateBooklistIntroRequest request)
        {
            using var conn = await _connectionFactory.CreateAsync();

            const string sql = @"
                UPDATE Booklist
                SET BooklistIntroduction = :NewIntro
                WHERE BooklistID = :BooklistID
                  AND CreatorID = :ReaderID
                  AND Status <> '删除'";

            var affected = await conn.ExecuteAsync(sql, new { BooklistID = booklistId, ReaderID = readerId, request.NewIntro });
            return new BooklistSuccessResponse { Success = affected };
        }
    }
}
