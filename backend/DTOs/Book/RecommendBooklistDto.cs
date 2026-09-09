namespace Backend.DTOs.Book
{
    public class RecommendBooklistDto
    {
        public int BooklistId { get; set; }
        public string ListCode { get; set; } = string.Empty;
        public string BooklistName { get; set; } = string.Empty;
        public string? BooklistIntroduction { get; set; }
        public int CreatorId { get; set; }
        public string CreatorNickname { get; set; } = string.Empty;
        public int MatchingBooksCount { get; set; }
        public decimal SimilarityScore { get; set; }
    }
}