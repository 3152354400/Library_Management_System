-- 收藏书单
CREATE OR REPLACE PROCEDURE CollectBooklist(
    p_BooklistID IN Booklist.BooklistID%TYPE,
    p_ReaderID IN Reader.ReaderID%TYPE,
    p_Notes IN Collect.Notes%TYPE DEFAULT NULL,
    p_Success OUT NUMBER
)
AS
    v_Count NUMBER;
BEGIN
    p_Success := 0;
    
    -- 检查书单是否存在
    SELECT COUNT(*) INTO v_Count FROM Booklist WHERE BooklistID = p_BooklistID;
    IF v_Count = 0 THEN
        RETURN;
    END IF;
    
    -- 检查是否已收藏
    SELECT COUNT(*) INTO v_Count 
    FROM Collect 
    WHERE BooklistID = p_BooklistID AND ReaderID = p_ReaderID;
    
    IF v_Count = 0 THEN
        INSERT INTO Collect (BooklistID, ReaderID, FavoriteTime, Notes)
        VALUES (p_BooklistID, p_ReaderID, SYSTIMESTAMP, p_Notes);
        
        p_Success := 1;
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE;
END;
/
