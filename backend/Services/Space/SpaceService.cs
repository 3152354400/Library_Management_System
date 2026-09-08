using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTOs.Reader;
using backend.Repositories.Space;

namespace backend.Services.Space
{
    public class SpaceService
    {
        private readonly SpaceRepository _repository;
        public SpaceService(SpaceRepository repository) { _repository = repository; }

        public Task<IEnumerable<SeatDto>> GetSeatLayoutAsync(int buildingId, int floor)
        {
            return _repository.GetSeatLayoutAsync(buildingId, floor);
        }

        public async Task CreateSeatReservationAsync(CreateSeatReservationDto dto, long readerId)
        {
            // 规则校验现在都在 Repository 的事务中处理
            // 我们只在 Service 层做最基本的时间逻辑校验
            if (dto.EndTime <= dto.StartTime)
                throw new InvalidOperationException("结束时间必须晚于开始时间。");

            if ((dto.EndTime - dto.StartTime).TotalHours > 4)
                throw new InvalidOperationException("单次预约最长为4小时。");

            // 直接调用新的事务性方法
            await _repository.CreateSeatReservationInTransactionAsync(dto.SeatID, readerId, dto.StartTime, dto.EndTime);
        }

        public Task<IEnumerable<MyReservationDto>> GetMyReservationsAsync(long readerId)
        {
            return _repository.GetMyReservationsAsync(readerId);
        }

        public Task<bool> CancelReservationAsync(int reservationId, long readerId)
        {
            return _repository.CancelReservationAsync(reservationId, readerId);
        }
    }
}