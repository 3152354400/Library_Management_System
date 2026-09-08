CREATE OR REPLACE FUNCTION get_recommendations(p_ReaderID INT, p_TopN INT)
RETURN SYS_REFCURSOR
AS
    v_cursor SYS_REFCURSOR;
BEGIN
OPEN v_cursor FOR
SELECT r.ISBN, bi.Title, bi.Author, r.BooklistCount
FROM book_list_rank_view r
         JOIN BookInfo bi ON r.ISBN = bi.ISBN
WHERE r.ReaderID = p_ReaderID
ORDER BY r.BooklistCount DESC
    FETCH FIRST p_TopN ROWS ONLY;
RETURN v_cursor;
END;
