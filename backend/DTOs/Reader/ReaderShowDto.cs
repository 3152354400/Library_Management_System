namespace backend.DTOs.Reader
{
    public class ReaderShowDto
    {
        public long ReaderID { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? NickName { get; set; }
        public int? CreditScore { get; set; }
        public string? AccountStatus { get; set; }
        public string? Permission { get; set; }
    }
}
