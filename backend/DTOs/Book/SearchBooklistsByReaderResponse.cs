using System;
using System.Collections.Generic;

namespace Backend.DTOs.Book
{
    public class SimpleBooklistDto
    {
        public int BooklistId { get; set; }
        public string ListCode { get; set; } = string.Empty;
        public string BooklistName { get; set; } = string.Empty;
        public string? BooklistIntroduction { get; set; }
        public int CreatorId { get; set; }
        public DateTime? FavoriteTime { get; set; }
    }

    public class SearchBooklistsByReaderResponse
    {
        public List<SimpleBooklistDto> Created { get; set; } = new();
        public List<SimpleBooklistDto> Collected { get; set; } = new();
    }
}