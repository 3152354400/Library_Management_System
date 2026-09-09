public class BookInfoDto
{
    public string? ISBN { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Categories { get; set; } // 分类信息，多个分类用逗号分隔
}

public class table_Book_Dto
{
    public int BookID { get; set; }
    public string? Barcode { get; set; } = "";
    public string? Status { get; set; } = ""; // '正常'/'下架'/'借出'
    public int? ShelfID { get; set; }
    public string? ISBN { get; set; } = "";
}

public static class table_Book_attribute_Status
{
    public const string Normal   = "正常";
    public const string Off      = "下架";
    public const string Borrowed = "借出";
}