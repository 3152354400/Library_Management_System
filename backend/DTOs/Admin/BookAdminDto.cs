namespace backend.DTOs.Admin
{
    // 用于向管理端前端展示图书列表
    public class BookAdminDto
    {
        public string ISBN { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int TotalCopies { get; set; }
        public int PhysicalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public int BorrowedCopies { get; set; }
        public int TakedownCopies { get; set; }
    }

    // 用于创建新图书
    public class CreateBookDto
    {
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int NumberOfCopies { get; set; } // 入库数量
    }

    // 用于更新图书信息
    public class UpdateBookDto
    {
        public string Title { get; set; }
        public string Author { get; set; }
    }
}