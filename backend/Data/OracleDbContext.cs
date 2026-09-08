using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Backend.Data
{
    // 数据库上下文类，连接 Oracle 并管理表结构
    public class OracleDbContext : DbContext
    {
        public OracleDbContext(DbContextOptions<OracleDbContext> options) : base(options) { }

        // 映射 books 表
        public DbSet<Book> Books { get; set; }
    }

    // 图书实体类，对应 Oracle 中的 books 表
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(100)]
        public string Author { get; set; }

        [MaxLength(100)]
        public string Publisher { get; set; }

        public DateTime? PublishedDate { get; set; }
    }
}
