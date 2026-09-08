--根据读者ID查询创建和收藏的书单
CREATE OR REPLACE PROCEDURE SearchBooklistsByReader(
    p_ReaderID IN Reader.ReaderID%TYPE,
    p_CreatedBooklists OUT SYS_REFCURSOR,
    p_CollectedBooklists OUT SYS_REFCURSOR
)
AS
BEGIN
    -- 返回读者创建的书单
    OPEN p_CreatedBooklists FOR
    SELECT b.BooklistID, b.ListCode, b.BooklistName, 
           b.BooklistIntroduction, b.CreatorID
    FROM Booklist b
    WHERE b.CreatorID = p_ReaderID
    ORDER BY b.BooklistID;

    -- 返回读者收藏的书单
    OPEN p_CollectedBooklists FOR
    SELECT b.BooklistID, b.ListCode, b.BooklistName, 
           b.BooklistIntroduction, b.CreatorID, c.FavoriteTime
    FROM Booklist b
    JOIN Collect c ON b.BooklistID = c.BooklistID
    WHERE c.ReaderID = p_ReaderID
    ORDER BY c.FavoriteTime DESC;
END;
/