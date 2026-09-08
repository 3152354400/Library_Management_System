using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTOs.Admin;

namespace backend.Repositories.Admin
{
    public class BookAdminRepository
    {
        private readonly string _connectionString;

        public BookAdminRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<BookAdminDto>> GetBooksAsync(string searchTerm)
        {
            var sql = "SELECT * FROM V_BookAdmin_Info";
            if (!string.IsNullOrEmpty(searchTerm))
            {
                sql += " WHERE LOWER(Title) LIKE :SearchTerm OR LOWER(Author) LIKE :SearchTerm OR ISBN LIKE :SearchTerm";
            }
            sql += " ORDER BY Title";
            
            using var connection = new OracleConnection(_connectionString);
            return await connection.QueryAsync<BookAdminDto>(sql, new { SearchTerm = $"%{searchTerm?.ToLower()}%" });
        }
        
        public async Task<bool> IsIsbnExistsAsync(string isbn)
        {
            var sql = "SELECT 1 FROM BookInfo WHERE ISBN = :ISBN";
            using var connection = new OracleConnection(_connectionString);
            var result = await connection.QueryFirstOrDefaultAsync<int?>(sql, new { ISBN = isbn });
            return result.HasValue;
        }

        public async Task CreateBookAsync(CreateBookDto dto)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            try
            {
                // Step 1: Insert into BookInfo
                var bookInfoSql = "INSERT INTO BookInfo (ISBN, Title, Author, Stock) VALUES (:ISBN, :Title, :Author, :Stock)";
                await connection.ExecuteAsync(bookInfoSql, new { dto.ISBN, dto.Title, dto.Author, Stock = dto.NumberOfCopies }, transaction);

                // Step 2: Insert multiple copies into Book
                if (dto.NumberOfCopies > 0)
                {
                    var bookSql = "INSERT INTO Book (Status, ISBN) VALUES ('正常', :ISBN)";
                    // Dapper can execute the same command multiple times with a list of parameters
                    var booksToInsert = new List<object>();
                    for (int i = 0; i < dto.NumberOfCopies; i++)
                    {
                        booksToInsert.Add(new { ISBN = dto.ISBN });
                    }
                    await connection.ExecuteAsync(bookSql, booksToInsert, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> UpdateBookInfoAsync(string isbn, UpdateBookDto dto)
        {
            var sql = "UPDATE BookInfo SET Title = :Title, Author = :Author WHERE ISBN = :ISBN";
            using var connection = new OracleConnection(_connectionString);
            var affectedRows = await connection.ExecuteAsync(sql, new { dto.Title, dto.Author, ISBN = isbn });
            return affectedRows > 0;
        }

        public async Task<bool> TakedownBookAsync(string isbn)
        {
            var sql = "UPDATE Book SET Status = '下架' WHERE ISBN = :ISBN AND Status = '正常'";
            using var connection = new OracleConnection(_connectionString);
            var affectedRows = await connection.ExecuteAsync(sql, new { ISBN = isbn });
            return affectedRows > 0;
        }

        public async Task AddCopiesAsync(AddCopiesDto dto)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            try
            {
                // 步骤 1: 在 Book 表中批量插入新的实体书副本
                if (dto.NumberOfCopies > 0)
                {
                    var bookSql = "INSERT INTO Book (Status, ShelfID, ISBN) VALUES ('正常', :ShelfID, :ISBN)";
                    var booksToInsert = new List<object>();
                    for (int i = 0; i < dto.NumberOfCopies; i++)
                    {
                        booksToInsert.Add(new { ShelfID = dto.ShelfID, ISBN = dto.ISBN });
                    }
                    await connection.ExecuteAsync(bookSql, booksToInsert, transaction);
                }

                // 步骤 2: 更新 BookInfo 表中的总库存量 (Stock)
                var bookInfoSql = "UPDATE BookInfo SET Stock = Stock + :NumberOfCopies WHERE ISBN = :ISBN";
                await connection.ExecuteAsync(bookInfoSql, new { dto.NumberOfCopies, dto.ISBN }, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}