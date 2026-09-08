namespace Backend.DTOs.Book
{
    public class CollectBooklistRequest
    {
        // public int BooklistId { get; set; } 从URL获取
        // public int ReaderId { get; set; }   从token获取
        public string? Notes { get; set; }
    }
}
