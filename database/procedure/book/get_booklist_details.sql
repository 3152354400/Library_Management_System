-- 获取书单详情
CREATE OR REPLACE PROCEDURE GetBooklistDetails(
    p_BooklistID IN Booklist.BooklistID%TYPE,
    p_BooklistInfo OUT SYS_REFCURSOR,
    p_BooksInfo OUT SYS_REFCURSOR
)
AS
BEGIN
    -- 返回书单基本信息
    OPEN p_BooklistInfo FOR
    SELECT b.BooklistID, b.ListCode, b.BooklistName, 
           b.BooklistIntroduction, b.CreatorID,
           r.Username AS CreatorUsername, r.Nickname AS CreatorNickname
    FROM Booklist b
    JOIN Reader r ON b.CreatorID = r.ReaderID
    WHERE b.BooklistID = p_BooklistID;
    
    -- 返回书单中的图书信息
    OPEN p_BooksInfo FOR
    SELECT bb.ISBN, bb.AddTime, bb.Notes,
           bi.Title, bi.Author
    FROM Booklist_Book bb
    JOIN BookInfo bi ON bb.ISBN = bi.ISBN
    WHERE bb.BooklistID = p_BooklistID
    ORDER BY bb.AddTime DESC;
END;
/