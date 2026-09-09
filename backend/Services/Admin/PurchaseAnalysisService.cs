using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTOs.Admin;
using backend.Repositories.Admin;

namespace backend.Services.Admin
{
    public class PurchaseAnalysisService
    {
        private readonly PurchaseAnalysisRepository _repository;

        public PurchaseAnalysisService(PurchaseAnalysisRepository repository)
        {
            _repository = repository;
        }

        public async Task<PurchaseAnalysisDto> GetPurchaseAnalysisAsync()
        {
            var analysis = new PurchaseAnalysisDto
            {
                TopByBorrowCount = await _repository.GetTop10ByBorrowCountAsync(),
                TopByBorrowDuration = await _repository.GetTop10ByBorrowDurationAsync(),
                TopByInstanceBorrow = await _repository.GetTop10ByReservationCountAsync()
            };
            return analysis;
        }

        public Task<IEnumerable<PurchaseLogDto>> GetPurchaseLogsAsync() => _repository.GetPurchaseLogsAsync();

        public Task AddPurchaseLogAsync(string logText) => _repository.AddPurchaseLogAsync(logText);
    }
}