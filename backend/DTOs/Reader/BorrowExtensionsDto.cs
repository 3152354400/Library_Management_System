namespace backend.DTOs.ReaderDtos
{
    /// <summary>
    /// 续借图书请求
    /// </summary>
    public class RenewBookRequest
    {
        /// <summary>
        /// 图书ID
        /// </summary>
        public string BookId { get; set; }
    }

    /// <summary>
    /// 续借结果响应
    /// </summary>
    public class RenewBookResponse
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 新应还时间
        /// </summary>
        public DateTime? NewDueTime { get; set; }
    }

    /// <summary>
    /// 预约图书请求
    /// </summary>
    public class ReserveBookRequest
    {
        /// <summary>
        /// ISBN
        /// </summary>
        public string ISBN { get; set; }

        /// <summary>
        /// 期望借书天数（可选，默认7天）
        /// </summary>
        public int ExpectedDays { get; set; } = 7;
    }

    /// <summary>
    /// 预约结果响应
    /// </summary>
    public class ReserveBookResponse
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 预约ID
        /// </summary>
        public int? ReserveId { get; set; }
    }

    /// <summary>
    /// 取消预约请求
    /// </summary>
    public class CancelReserveRequest
    {
        /// <summary>
        /// 预约ID
        /// </summary>
        public int ReserveId { get; set; }

        /// <summary>
        /// 取消原因
        /// </summary>
        public string CancelReason { get; set; }
    }

    /// <summary>
    /// 预约记录DTO
    /// </summary>
    public class ReserveRecordDto
    {
        public int ReserveId { get; set; }
        public string ISBN { get; set; }
        public string BookTitle { get; set; }
        public string Author { get; set; }
        public DateTime ReserveTime { get; set; }
        public DateTime ExpectedBorrowTime { get; set; }
        public DateTime ExpireTime { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>
    /// 罚款记录DTO
    /// </summary>
    public class FineRecordDto
    {
        public int FineId { get; set; }
        public string FineType { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal UnpaidAmount { get; set; }
        public DateTime FineDate { get; set; }
        public string PayStatus { get; set; }
        public string BookTitle { get; set; }
        public string ISBN { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>
    /// 罚款汇总DTO
    /// </summary>
    public class FineSummaryDto
    {
        public int TotalFines { get; set; }
        public int UnpaidFines { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal UnpaidAmount { get; set; }
    }

    /// <summary>
    /// 缴纳罚款请求
    /// </summary>
    public class PayFineRequest
    {
        /// <summary>
        /// 罚款ID
        /// </summary>
        public int FineId { get; set; }

        /// <summary>
        /// 缴纳金额
        /// </summary>
        public decimal PayAmount { get; set; }

        /// <summary>
        /// 缴纳方式
        /// </summary>
        public string PayMethod { get; set; }
    }
}
