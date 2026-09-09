using Microsoft.AspNetCore.Mvc;
using Dapper;
using backend.Services.Web;
using backend.Models;

namespace backend.Controllers.ReaderFeature
{
    /// <summary>
    /// 图书收藏控制器 - 收藏具体的图书
    /// </summary>
    [ApiController]
    [Route("api/reader/favorite")]
    public class BookFavoriteController : ControllerBase
    {
        private readonly SecurityService _securityService;
        private readonly string _connectionString;

        public BookFavoriteController(SecurityService securityService, IConfiguration configuration)
        {
            _securityService = securityService;
            _connectionString = configuration.GetConnectionString("OracleDB") ?? "";
        }

        private int GetCurrentReaderId()
        {
            var loginUser = _securityService.GetLoginUser();
            if (_securityService.CheckIsReader(loginUser))
            {
                var reader = loginUser.User as Reader;
                return (int)reader.ReaderID;
            }
            return 0;
        }

        /// <summary>
        /// 获取当前读者的所有收藏
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetMyFavorites([FromQuery] string folderName = null)
        {
            var readerId = GetCurrentReaderId();
            if (readerId == 0) return Unauthorized(new { message = "请以读者身份登录" });

            using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                SELECT 
                    f.FavoriteID, f.ReaderID, f.ISBN, f.FavoriteTime, f.Notes, f.FolderName,
                    bi.Title, bi.Author, bi.Publisher, bi.PublishYear,
                    (SELECT COUNT(*) FROM Book b WHERE b.ISBN = bi.ISBN) AS TotalStock,
                    (SELECT COUNT(*) FROM Book b WHERE b.ISBN = bi.ISBN AND b.Status = '正常') AS AvailableStock
                FROM BookFavorite f
                JOIN BookInfo bi ON f.ISBN = bi.ISBN
                WHERE f.ReaderID = :readerId";

            if (!string.IsNullOrEmpty(folderName))
            {
                sql += " AND f.FolderName = :folderName";
            }
            sql += " ORDER BY f.FavoriteTime DESC";

            var favorites = await connection.QueryAsync(sql, new { readerId, folderName });
            return Ok(favorites);
        }

        /// <summary>
        /// 检查某本书是否已收藏
        /// </summary>
        [HttpGet("check")]
        public async Task<IActionResult> CheckFavorite([FromQuery] string isbn)
        {
            var readerId = GetCurrentReaderId();
            if (readerId == 0) return Unauthorized(new { message = "请以读者身份登录" });

            using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
            await connection.OpenAsync();

            var exists = await connection.ExecuteScalarAsync<bool>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM BookFavorite WHERE ReaderID = :readerId AND ISBN = :isbn) THEN 1 ELSE 0 END FROM DUAL",
                new { readerId, isbn });

            return Ok(new { isFavorite = exists });
        }

        /// <summary>
        /// 收藏图书
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddFavorite([FromBody] AddFavoriteRequest request)
        {
            var readerId = GetCurrentReaderId();
            if (readerId == 0) return Unauthorized(new { message = "请以读者身份登录" });

            if (string.IsNullOrEmpty(request.ISBN))
                return BadRequest(new { message = "ISBN不能为空" });

            using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
            await connection.OpenAsync();

            // 检查是否已收藏
            var exists = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM BookFavorite WHERE ReaderID = :readerId AND ISBN = :isbn",
                new { readerId, isbn = request.ISBN });

            if (exists > 0)
                return BadRequest(new { message = "该图书已在收藏列表中" });

            // 检查书是否存在
            var bookExists = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM BookInfo WHERE ISBN = :isbn",
                new { isbn = request.ISBN });

            if (bookExists == 0)
                return NotFound(new { message = "该图书不存在" });

            var favoriteId = await connection.ExecuteScalarAsync<int>(
                "SELECT NVL(MAX(FavoriteID), 0) + 1 FROM BookFavorite");

            await connection.ExecuteAsync(@"
                INSERT INTO BookFavorite (FavoriteID, ReaderID, ISBN, Notes, FolderName)
                VALUES (:favoriteId, :readerId, :isbn, :notes, :folderName)",
                new
                {
                    favoriteId,
                    readerId,
                    isbn = request.ISBN,
                    notes = request.Notes ?? "",
                    folderName = request.FolderName ?? "默认收藏夹"
                });

            return Ok(new { message = "收藏成功", favoriteId });
        }

        /// <summary>
        /// 取消收藏
        /// </summary>
        [HttpDelete("{favoriteId}")]
        public async Task<IActionResult> RemoveFavorite(int favoriteId)
        {
            var readerId = GetCurrentReaderId();
            if (readerId == 0) return Unauthorized(new { message = "请以读者身份登录" });

            using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
            await connection.OpenAsync();

            var rows = await connection.ExecuteAsync(
                "DELETE FROM BookFavorite WHERE FavoriteID = :favoriteId AND ReaderID = :readerId",
                new { favoriteId, readerId });

            if (rows == 0) return NotFound(new { message = "收藏记录不存在" });

            return Ok(new { message = "取消收藏成功" });
        }

        /// <summary>
        /// 通过ISBN取消收藏
        /// </summary>
        [HttpDelete("by-isbn")]
        public async Task<IActionResult> RemoveFavoriteByISBN([FromQuery] string isbn)
        {
            var readerId = GetCurrentReaderId();
            if (readerId == 0) return Unauthorized(new { message = "请以读者身份登录" });

            using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
            await connection.OpenAsync();

            var rows = await connection.ExecuteAsync(
                "DELETE FROM BookFavorite WHERE ReaderID = :readerId AND ISBN = :isbn",
                new { readerId, isbn });

            if (rows == 0) return NotFound(new { message = "收藏记录不存在" });

            return Ok(new { message = "取消收藏成功" });
        }

        /// <summary>
        /// 更新收藏备注
        /// </summary>
        [HttpPut("{favoriteId}/notes")]
        public async Task<IActionResult> UpdateNotes(int favoriteId, [FromBody] UpdateFavoriteNotesRequest request)
        {
            var readerId = GetCurrentReaderId();
            if (readerId == 0) return Unauthorized(new { message = "请以读者身份登录" });

            using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
            await connection.OpenAsync();

            var rows = await connection.ExecuteAsync(
                "UPDATE BookFavorite SET Notes = :notes, FolderName = :folderName WHERE FavoriteID = :favoriteId AND ReaderID = :readerId",
                new { notes = request.Notes ?? "", folderName = request.FolderName ?? "默认收藏夹", favoriteId, readerId });

            if (rows == 0) return NotFound(new { message = "收藏记录不存在" });

            return Ok(new { message = "更新成功" });
        }

        /// <summary>
        /// 获取收藏夹列表（按收藏夹分组）
        /// </summary>
        [HttpGet("folders")]
        public async Task<IActionResult> GetFolders()
        {
            var readerId = GetCurrentReaderId();
            if (readerId == 0) return Unauthorized(new { message = "请以读者身份登录" });

            using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
            await connection.OpenAsync();

            var folders = await connection.QueryAsync(
                @"SELECT FolderName, COUNT(*) AS Count 
                  FROM BookFavorite 
                  WHERE ReaderID = :readerId 
                  GROUP BY FolderName 
                  ORDER BY FolderName",
                new { readerId });

            return Ok(folders);
        }
    }

    public class AddFavoriteRequest
    {
        public string ISBN { get; set; }
        public string Notes { get; set; }
        public string FolderName { get; set; }
    }

    public class UpdateFavoriteNotesRequest
    {
        public string Notes { get; set; }
        public string FolderName { get; set; }
    }
}
