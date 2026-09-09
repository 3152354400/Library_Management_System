using System;

namespace Backend.DTOs.Book
{
    public class BookItemDto
    {
        public string ISBN { get; set; } = string.Empty;
        public DateTime AddTime { get; set; }
        public string? Notes { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
    }
}
