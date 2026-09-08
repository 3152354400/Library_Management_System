namespace backend.DTOs
{
    public class RecommendedBookDto
    {
        public string ISBN { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int BooklistCount { get; set; }
    }
}
