using System;

namespace Backend.DTOs.Book
{
    public class BooklistInfoDto
    {
        public int BooklistId { get; set; }
        public string ListCode { get; set; } = string.Empty;
        public string BooklistName { get; set; } = string.Empty;
        public string? BooklistIntroduction { get; set; }
        public int CreatorId { get; set; }
        public string CreatorUsername { get; set; } = string.Empty;
        public string CreatorNickname { get; set; } = string.Empty;
    }
}
