using System.Collections.Generic;

namespace Backend.DTOs.Book
{
    public class RecommendBooklistsResponse
    {
        public List<RecommendBooklistDto> Items { get; set; } = new();
    }
}