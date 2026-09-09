using System;

namespace backend.Models
{
    public class BorrowRecord
    {
        public int BorrowRecordID { get; set; }

        public int ReaderID { get; set; }

        public int BookID { get; set; }

        public DateTime BorrowTime { get; set; }

        public DateTime DueTime { get; set; }

        public DateTime? ReturnTime { get; set; }

        public decimal OverdueFine { get; set; }

        public string Status { get; set; } = "借阅中";

        public int BorrowRecordId
        {
            get => BorrowRecordID;
            set => BorrowRecordID = value;
        }

        public string ReaderId
        {
            get => ReaderID.ToString();
            set => ReaderID = int.Parse(value);
        }

        public string BookId
        {
            get => BookID.ToString();
            set => BookID = int.Parse(value);
        }
    }
}
