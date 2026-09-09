public class CommentDetailDto
{
    public decimal? CommentID { get; set; }
    public decimal? ReaderID { get; set; }
    public string? ISBN { get; set; }
    public int? RATING { get; set; }
    public required string ReviewContent { get; set; }
    public DateTime CreateTime { get; set; }
    public string Status { get; set; } = "正常";
}

public class ReportDto
{
    public decimal? ReportID { get; set; }
    public required decimal CommentID { get; set; }
    public required decimal ReaderID { get; set; }
    public string ReportReason { get; set; } = string.Empty;
    public DateTime ReportTime{ get; set; }
    public string Status { get; set; } = "待处理";
    public decimal LibrarianID { get; set; }
}

// public class ReportStatusDto
// {
//     public decimal ReportID { get; set; }
//     public string Status { get; set; } = "待处理";
// }