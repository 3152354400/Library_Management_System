using System;

namespace backend.DTOs.Admin
{
    public class ReportDetailDto
    {
        public int ReportID { get; set; }
        public string ReportReason { get; set; } = string.Empty;
        public DateTime ReportTime { get; set; }
        public string ReportStatus { get; set; } = string.Empty;

        public int CommentID { get; set; }
        public string ReviewContent { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CommentTime { get; set; }
        public string CommentStatus { get; set; } = string.Empty;

        public int ReporterID { get; set; }
        public string ReporterUsername { get; set; } = string.Empty;
        public string ReporterNickname => ReporterUsername;

        public int CommenterID { get; set; }
        public string CommenterNickname { get; set; } = string.Empty;
        public string CommenterAccountStatus { get; set; } = string.Empty;

        public string ISBN { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
    }

    public class ReportDto
    {
        public int ReportID { get; set; }
        public int CommentID { get; set; }
        public int ReaderID { get; set; }
        public string ReportReason { get; set; } = string.Empty;
        public DateTime ReportTime { get; set; } = DateTime.Now;
        public string Status { get; set; } = "待处理";
        public int? LibrarianID { get; set; }
    }
}
