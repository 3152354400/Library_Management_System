namespace backend.Models
{
    /// <summary>
    /// 图书分类关联模型
    /// </summary>
    public class BookCategory
    {
        /// <summary>
        /// 图书ISBN
        /// </summary>
        public string ISBN { get; set; } = string.Empty;

        /// <summary>
        /// 分类ID
        /// </summary>
        public string CategoryID { get; set; } = string.Empty;

        /// <summary>
        /// 关联备注
        /// </summary>
        public string? RelationNote { get; set; }
    }
}
