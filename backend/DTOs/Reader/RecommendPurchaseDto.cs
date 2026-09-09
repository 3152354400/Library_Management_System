namespace backend.DTOs.ReaderDtos
{
    /// <summary>
    /// 荐购图书请求
    /// </summary>
    public class RecommendPurchaseRequest
    {
        /// <summary>
        /// ISBN（可选）
        /// </summary>
        public string ISBN { get; set; }

        /// <summary>
        /// 书名
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// 出版社
        /// </summary>
        public string Publisher { get; set; }

        /// <summary>
        /// 出版年份
        /// </summary>
        public int? PublishYear { get; set; }

        /// <summary>
        /// 推荐理由
        /// </summary>
        public string Reason { get; set; }
    }

    /// <summary>
    /// 荐购结果响应
    /// </summary>
    public class RecommendPurchaseResponse
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
        /// 荐购ID
        /// </summary>
        public int? RecommendId { get; set; }
    }

    /// <summary>
    /// 荐购记录DTO
    /// </summary>
    public class RecommendPurchaseDto
    {
        public int RecommendId { get; set; }
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public int? PublishYear { get; set; }
        public string Reason { get; set; }
        public DateTime RecommendTime { get; set; }
        public string Status { get; set; }
        public DateTime? HandleTime { get; set; }
        public string HandleResult { get; set; }
    }

    /// <summary>
    /// 通知消息DTO
    /// </summary>
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        public string Priority { get; set; }
        public DateTime CreateTime { get; set; }
        public string IsRead { get; set; }
        public DateTime? ReadTime { get; set; }
        public string RelatedType { get; set; }
        public int? RelatedId { get; set; }
    }

    /// <summary>
    /// 通知列表响应
    /// </summary>
    public class NotificationListResponse
    {
        /// <summary>
        /// 通知列表
        /// </summary>
        public List<NotificationDto> Notifications { get; set; }

        /// <summary>
        /// 总数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 未读数量
        /// </summary>
        public int UnreadCount { get; set; }
    }
}
