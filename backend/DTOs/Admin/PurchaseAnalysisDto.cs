using System.Collections.Generic;

namespace backend.DTOs.Admin
{
    public class PurchaseAnalysisDto
    {
        public IEnumerable<BookRankingDto> TopByBorrowCount { get; set; }
        public IEnumerable<BookRankingDto> TopByBorrowDuration { get; set; }
        public IEnumerable<BookRankingDto> TopByInstanceBorrow { get; set; }
    }
}