-- 取消收藏书单
CREATE OR REPLACE PROCEDURE CancelCollectBooklist(
    p_BooklistID IN Booklist.BooklistID%TYPE,
    p_ReaderID IN Reader.ReaderID%TYPE,
    p_Success OUT NUMBER
)
AS
BEGIN
    p_Success := 0;
    
    DELETE FROM Collect 
    WHERE BooklistID = p_BooklistID AND ReaderID = p_ReaderID;
    
    IF SQL%ROWCOUNT > 0 THEN
        p_Success := 1;
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE;
END;
/