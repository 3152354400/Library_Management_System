namespace backend.DTOs.Book
{
    /// <summary>
    /// 分类模型
    /// </summary>
    public class Category
    {
        public string CategoryID { get; set; } = string.Empty;
        public required string CategoryName { get; set; }
        public string? ParentCategoryID { get; set; }
    }

    /// <summary>
    /// 图书分类关联请求DTO
    /// </summary>
    public class BookCategoryRequestDto
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

    /// <summary>
    /// 图书分类关联详情DTO
    /// </summary>
    public class BookCategoryDetailDto
    {
        /// <summary>
        /// 图书ISBN
        /// </summary>
        public string ISBN { get; set; } = string.Empty;

        /// <summary>
        /// 图书标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 图书作者
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// 分类ID
        /// </summary>
        public string CategoryID { get; set; } = string.Empty;

        /// <summary>
        /// 分类名称
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// 分类完整路径
        /// </summary>
        public string CategoryPath { get; set; } = string.Empty;

        /// <summary>
        /// 关联备注
        /// </summary>
        public string? RelationNote { get; set; }
    }

    /// <summary>
    /// 图书分类绑定请求DTO
    /// </summary>
    public class BookCategoryBindDto
    {
        /// <summary>
        /// 图书ISBN
        /// </summary>
        public string ISBN { get; set; } = string.Empty;

        /// <summary>
        /// 分类ID列表
        /// </summary>
        public List<string> CategoryIDs { get; set; } = new List<string>();

        /// <summary>
        /// 关联备注
        /// </summary>
        public string? RelationNote { get; set; }
    }

    /// <summary>
    /// 分类信息DTO（用于选择器）
    /// </summary>
    public class CategorySelectDto
    {
        /// <summary>
        /// 分类ID
        /// </summary>
        public string CategoryID { get; set; } = string.Empty;

        /// <summary>
        /// 分类名称
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// 分类完整路径
        /// </summary>
        public string CategoryPath { get; set; } = string.Empty;

        /// <summary>
        /// 是否为叶子节点
        /// </summary>
        public bool IsLeaf { get; set; }
    }
}