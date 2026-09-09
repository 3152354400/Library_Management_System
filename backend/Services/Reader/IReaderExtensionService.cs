using System.Threading.Tasks;
using backend.DTOs;
using backend.DTOs.ReaderDtos;
using backend.Repositories.ReaderRepository;

namespace backend.Services.ReaderService
{
    /// <summary>
    /// 读者扩展服务接口
    /// </summary>
    public interface IReaderExtensionService
    {
        /// <summary>
        /// 续借图书
        /// </summary>
        Task<RenewBookResponse> RenewBookAsync(int readerId, string bookId);

        /// <summary>
        /// 获取读者罚款列表
        /// </summary>
        Task<List<FineRecordDto>> GetReaderFinesAsync(int readerId);

        /// <summary>
        /// 获取读者罚款列表（分页）
        /// </summary>
        Task<PagedResult<FineRecordDto>> GetReaderFinesPagedAsync(int readerId, int pageSize, int pageNum, string status = null);

        /// <summary>
        /// 获取读者罚款汇总
        /// </summary>
        Task<FineSummaryDto> GetReaderFineSummaryAsync(int readerId);

        /// <summary>
        /// 预约图书
        /// </summary>
        Task<ReserveBookResponse> ReserveBookAsync(int readerId, ReserveBookRequest request);

        /// <summary>
        /// 取消预约
        /// </summary>
        Task<string> CancelReserveAsync(int readerId, int reserveId, string reason);

        /// <summary>
        /// 获取读者预约列表
        /// </summary>
        Task<List<ReserveRecordDto>> GetReaderReservesAsync(int readerId);

        /// <summary>
        /// 荐购图书
        /// </summary>
        Task<RecommendPurchaseResponse> RecommendPurchaseAsync(int readerId, RecommendPurchaseRequest request);

        /// <summary>
        /// 获取读者荐购列表
        /// </summary>
        Task<List<RecommendPurchaseDto>> GetReaderRecommendsAsync(int readerId);

        /// <summary>
        /// 获取读者消息通知
        /// </summary>
        Task<NotificationListResponse> GetReaderNotificationsAsync(int readerId, string isRead = null, string type = null, int pageSize = 20, int pageNum = 1);

        /// <summary>
        /// 获取未读通知数量
        /// </summary>
        Task<int> GetUnreadNotificationCountAsync(int readerId);

        /// <summary>
        /// 标记通知为已读
        /// </summary>
        Task<bool> MarkNotificationReadAsync(int readerId, int notificationId);

        /// <summary>
        /// 标记所有通知为已读
        /// </summary>
        Task<int> MarkAllNotificationsReadAsync(int readerId);
    }
}
