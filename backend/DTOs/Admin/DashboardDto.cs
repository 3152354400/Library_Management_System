using System.Collections.Generic;

namespace backend.DTOs.Admin
{
    public class DashboardStatsDto
    {
        public int TotalBooks { get; set; }
        public int TotalReaders { get; set; }
        public int TodayBorrows { get; set; }
        public int TodayReturns { get; set; }
        public int OverdueCount { get; set; }
        public decimal PendingFines { get; set; }
        public int SeatAvailable { get; set; }
        public int NewBooksThisWeek { get; set; }
        public int TodoRecommends { get; set; }
        public int TodoReports { get; set; }
        public int TodoFineAppeals { get; set; }
        public int TodoBookLoss { get; set; }
        public List<DashboardTrendItemDto> Trend { get; set; } = new List<DashboardTrendItemDto>();
        public List<DashboardHotBookDto> HotBooks { get; set; } = new List<DashboardHotBookDto>();
    }

    public class DashboardTrendItemDto
    {
        public string Date { get; set; }
        public int Count { get; set; }
    }

    public class DashboardHotBookDto
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public int BorrowCount { get; set; }
    }
}
