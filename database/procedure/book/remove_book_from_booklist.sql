-- 在书单中删除书籍
CREATE OR REPLACE PROCEDURE RemoveBookFromBooklist(
    p_BooklistID IN Booklist.BooklistID%TYPE,
    p_ISBN IN BookInfo.ISBN%TYPE,
    p_Success OUT NUMBER
)
AS
BEGIN
    p_Success := 0;
    
    DELETE FROM Booklist_Book 
    WHERE BooklistID = p_BooklistID AND ISBN = p_ISBN;
    
    IF SQL%ROWCOUNT > 0 THEN
        p_Success := 1;
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE;
END;
/