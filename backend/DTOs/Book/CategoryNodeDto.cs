public class CategoryNode
{
    public string CategoryID { get; set; } = string.Empty;
    public required string CategoryName { get; set; }
    public string? ParentCategoryID { get; set; }
    public List<CategoryNode> Children { get; set; } = new List<CategoryNode>();
} 