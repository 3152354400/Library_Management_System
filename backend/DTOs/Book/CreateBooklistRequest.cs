namespace Backend.DTOs.Book
{
    public class CreateBooklistRequest
    {
        public string BooklistName { get; set; } = string.Empty;
        public string? BooklistIntroduction { get; set; }
        // public int CreatorId { get; set; }  从token获取
    }
}