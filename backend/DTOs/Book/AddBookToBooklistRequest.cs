namespace Backend.DTOs.Book
{
    public class AddBookToBooklistRequest
    {
        // public int BooklistId { get; set; }  从URL获取
        public string ISBN { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}