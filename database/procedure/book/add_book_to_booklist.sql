-- 在书单中添加书籍
CREATE OR REPLACE PROCEDURE AddBookToBooklist(
    p_BooklistID IN Booklist.BooklistID%TYPE,
    p_ISBN IN BookInfo.ISBN%TYPE,
    p_Notes IN Booklist_Book.Notes%TYPE DEFAULT NULL,
    p_Success OUT NUMBER
)
AS
    v_Count NUMBER;
BEGIN
    p_Success := 0;
    
    -- 检查图书是否存在
    SELECT COUNT(*) INTO v_Count FROM BookInfo WHERE ISBN = p_ISBN;
    IF v_Count = 0 THEN
        RETURN;
    END IF;
    
    -- 检查书单是否存在
    SELECT COUNT(*) INTO v_Count FROM Booklist WHERE BooklistID = p_BooklistID;
    IF v_Count = 0 THEN
        RETURN;
    END IF;
    
    -- 检查是否已添加过
    SELECT COUNT(*) INTO v_Count 
    FROM Booklist_Book 
    WHERE BooklistID = p_BooklistID AND ISBN = p_ISBN;
    
    IF v_Count = 0 THEN
        INSERT INTO Booklist_Book (BooklistID, ISBN, AddTime, Notes)
        VALUES (p_BooklistID, p_ISBN, SYSTIMESTAMP, p_Notes);
        
        p_Success := 1;
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE;
END;
/