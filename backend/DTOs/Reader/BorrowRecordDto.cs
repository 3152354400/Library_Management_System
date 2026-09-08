using System;

namespace backend.DTOs
{
    public class BorrowRecordDto
    {
        public int BorrowRecordID { get; set; }

        public required string ReaderId { get; set; }

        public required string BookId { get; set; }

        public string? ReaderName { get; set; }

        public string? BookName { get; set; }

        public DateTime BorrowTime { get; set; }

        public DateTime DueTime { get; set; }

        public DateTime? ReturnTime { get; set; }

        public decimal OverdueFine { get; set; }

        public string Status { get; set; } = "借阅中";
    }
}
