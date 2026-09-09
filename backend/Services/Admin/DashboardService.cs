using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTOs.Admin;
using backend.Repositories.Admin;

namespace backend.Services.Admin
{
    public class DashboardService
    {
        private readonly DashboardRepository _repository;

        public DashboardService(DashboardRepository repository)
        {
            _repository = repository;
        }

        public async Task<DashboardStatsDto> GetStatsAsync()
        {
            var dto = new DashboardStatsDto
            {
                TotalBooks = await _repository.ScalarIntAsync("SELECT COUNT(*) FROM BOOK"),
                TotalReaders = await _repository.ScalarIntAsync("SELECT COUNT(*) FROM READER"),
                TodayBorrows = await _repository.ScalarIntAsync("SELECT COUNT(*) FROM BORROWRECORD WHERE TRUNC(BORROWTIME) = TRUNC(SYSDATE)"),
                TodayReturns = await _repository.ScalarIntAsync("SELECT COUNT(*) FROM BORROWRECORD WHERE RETURNTIME IS NOT NULL AND TRUNC(RETURNTIME) = TRUNC(SYSDATE)"),
                OverdueCount = await _repository.ScalarIntAsync("SELECT COUNT(*) FROM BORROWRECORD WHERE STATUS = '逾期'"),
                PendingFines = await _repository.ScalarDecimalAsync("SELECT NVL(SUM(AMOUNT - NVL(PAIDAMOUNT, 0)), 0) FROM FINE"),
                SeatAvailable = await _repository.ScalarIntAsync("SELECT COUNT(*) FROM SEAT WHERE RESERVATIONSTATUS = '空闲'"),
                NewBooksThisWeek = await _repository.ScalarIntAsync("SELECT COUNT(*) FROM BOOK WHERE INDATE >= TRUNC(SYSDATE, 'IW')"),
                TodoRecommends = await _repository.ScalarIntAsync("SELECT COUNT(*) FROM RECOMMENDPURCHASE WHERE STATUS = '待审核'"),
                TodoReports = await _repository.ScalarIntAsync("SELECT COUNT(*) FROM REPORT WHERE STATUS = '待处理'"),
                TodoFineAppeals = await _repository.ScalarIntAsync("SELECT COUNT(*) FROM FINE WHERE FINETYPE = '减免申请'"),
                TodoBookLoss = await _repository.ScalarIntAsync("SELECT COUNT(*) FROM BOOKLOSSREPORT WHERE STATUS = '待处理'")
            };

            var trend = (await _repository.GetTrendAsync()).ToList();
            var list = new List<DashboardTrendItemDto>();
            for (int i = 6; i >= 0; i--)
            {
                var day = DateTime.Today.AddDays(-i);
                var key = day.ToString("MM-dd");
                var item = trend.FirstOrDefault(t => string.Equals(t.Date, key, StringComparison.OrdinalIgnoreCase));
                list.Add(new DashboardTrendItemDto { Date = key, Count = item?.Count ?? 0 });
            }
            dto.Trend = list;

            dto.HotBooks = (await _repository.GetHotBooksAsync()).ToList();
            return dto;
        }
    }
}
