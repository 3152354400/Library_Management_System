using System;
using System.Collections.Generic;

namespace backend.DTOs.Admin
{
    public class AnnouncementDto
    {
        public int AnnouncementID { get; set; }
        public int LibrarianID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreateTime { get; set; }
        public string TargetGroup { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class UpsertAnnouncementDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string TargetGroup { get; set; } = "所有人";
    }

    public class PublicAnnouncementsDto
    {
        public IEnumerable<AnnouncementDto> Urgent { get; set; } = new List<AnnouncementDto>();
        public IEnumerable<AnnouncementDto> Regular { get; set; } = new List<AnnouncementDto>();
    }
}
