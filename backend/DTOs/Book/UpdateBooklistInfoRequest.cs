namespace Backend.DTOs.Book
{
    public class UpdateBooklistNameRequest
    {
        // public int BooklistId { get; set; } 从URL获取
        // public int ReaderId { get; set; }   从token获取
        public string NewName { get; set; } = string.Empty;
    }

    public class UpdateBooklistIntroRequest
    {
        // public int BooklistId { get; set; } 从URL获取
        // public int ReaderId { get; set; }   从token获取
        public string? NewIntro { get; set; }
    }
}