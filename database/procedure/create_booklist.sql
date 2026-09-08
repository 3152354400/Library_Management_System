-- 创建书单
CREATE OR REPLACE PROCEDURE CreateBooklist(
    p_BooklistName IN Booklist.BooklistName%TYPE,
    p_BooklistIntroduction IN Booklist.BooklistIntroduction%TYPE,
    p_CreatorID IN Booklist.CreatorID%TYPE,
    p_BooklistID OUT Booklist.BooklistID%TYPE,
    p_Success OUT NUMBER
)
AS
    v_ListCode Booklist.ListCode%TYPE;
    v_ReaderCount NUMBER;
BEGIN
    p_Success := 0;
    
    -- 检查创建者是否存在
    SELECT COUNT(*) INTO v_ReaderCount FROM Reader WHERE ReaderID = p_CreatorID;
    IF v_ReaderCount = 0 THEN
        RETURN;
    END IF;
    
    -- 生成唯一书单代码
    v_ListCode := GenerateListCode();
    
    -- 插入书单记录
    INSERT INTO Booklist (ListCode, BooklistName, BooklistIntroduction, CreatorID)
    VALUES (v_ListCode, p_BooklistName, p_BooklistIntroduction, p_CreatorID)
    RETURNING BooklistID INTO p_BooklistID;
    
    p_Success := 1;
EXCEPTION
    WHEN OTHERS THEN
        p_Success := 0;
        RAISE;
END;
/