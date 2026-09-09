using System;
using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class BorrowRecordDetailDto
    {
        public int BorrowRecordID { get; set; }

        public int BookID { get; set; }

        public string? Barcode { get; set; }

        public string? ISBN { get; set; }

        public string? Title { get; set; }

        public string? Author { get; set; }

        public int ReaderID { get; set; }

        public string? Username { get; set; }

        public string? Fullname { get; set; }

        public int CreditScore { get; set; }

        public string? AccountStatus { get; set; }

        public int? ShelfID { get; set; }

        public string? ShelfCode { get; set; }

        public int? Floor { get; set; }

        public string? Zone { get; set; }

        public int? BuildingID { get; set; }

        public string? BuildingName { get; set; }

        public DateTime BorrowTime { get; set; }

        public DateTime DueTime { get; set; }

        public DateTime? ReturnTime { get; set; }

        public decimal OverdueFine { get; set; }

        public string? BorrowStatus { get; set; }

        public string? BookStatus { get; set; }

        [JsonIgnore]
        public string? BookId => BookID.ToString();

        public string? BookTitle => Title;

        public string? BookAuthor => Author;

        [JsonIgnore]
        public string? ReaderId => ReaderID.ToString();

        public string? ReaderName => Fullname ?? Username;
    }
}
