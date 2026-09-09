namespace Backend.DTOs.Book
{
    public class UpdateCollectNotesRequest
    {
        // public int BooklistId { get; set; } 从URL中获取
        // public int ReaderId { get; set; }   从token中获取
        public string? NewNotes { get; set; }
    }
}