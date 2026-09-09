-- 删除书单
CREATE OR REPLACE PROCEDURE DeleteBooklist(
    p_BooklistID IN Booklist.BooklistID%TYPE,
    p_ReaderID IN Reader.ReaderID%TYPE,
    p_Success OUT NUMBER
)
AS
    v_CreatorID Booklist.CreatorID%TYPE;
BEGIN
    p_Success := 0;
    
    -- 检查书单是否存在且用户有权限删除
    SELECT CreatorID INTO v_CreatorID 
    FROM Booklist 
    WHERE BooklistID = p_BooklistID;
    
    IF v_CreatorID = p_ReaderID THEN
        -- 先删除相关收藏记录
        DELETE FROM Collect WHERE BooklistID = p_BooklistID;
        -- 再删除书单-图书关联记录
        DELETE FROM Booklist_Book WHERE BooklistID = p_BooklistID;
        -- 最后删除书单
        DELETE FROM Booklist WHERE BooklistID = p_BooklistID;
        
        p_Success := 1;
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_Success := 0;
    WHEN OTHERS THEN
        RAISE;
END;
/