

public class BookDto
{

    public string? TITLE { get; set; }    // 书名（来自BookInfo表）
    public string? BUILDINGID { get; set; }  // 书架ID（来自Book表）
    public string? SHELFCODE { get; set; }
    public string? FLOOR { get; set; }
    public string? ZONE { get; set; }
    public string? STATUS { get; set; }
    public string? BOOKID { get; set; }
    public string? BARCODE { get; set; }
}

public class BookShelf
{

    public string? SHELFID { get; set; }    // 书名（来自BookInfo表）
    public string? BUILDINGID { get; set; }  // 书架ID（来自Book表）
    public string? SHELFCODE { get; set; }
    public string? FLOOR { get; set; }
    public string? ZONE { get; set; }

}

// BookShelfInsertDto.cs
public class BookShelfInsertDto
{
    public int BUILDINGID { get; set; }
    public string? SHELFCODE { get; set; }  // 添加 ?
    public int FLOOR { get; set; }
    public string? ZONE { get; set; }      // 添加 ?
}


public class BookReturnDto
{
    public int BookId { get; set; }
    public int ShelfId { get; set; }
}

// 添加借出DTO
public class BookBorrowDto
{
    public int BookId { get; set; }
}

// ShelfBookDto.cs
public class ShelfBookDto
{
    public int BOOKID { get; set; }
    public string? TITLE { get; set; }
    public string? AUTHOR { get; set; }
    public string? STATUS { get; set; }
    public string? BARCODE { get; set; }
}