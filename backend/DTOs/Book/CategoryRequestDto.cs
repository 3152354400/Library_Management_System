namespace backend.DTOs.Book
{
    public class CategoryRequest
    {
        public required Category Category { get; set; }
        public required string OperatorId { get; set; }
    }
} 