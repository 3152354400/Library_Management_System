-- ================================================================
-- 查询读者罚款列表
-- 支持读者查询自己的罚款记录
-- ================================================================
CREATE OR REPLACE PROCEDURE sp_get_reader_fines(
    p_reader_id IN INT,
    p_paging_start IN INT DEFAULT 1,
    p_paging_end IN INT DEFAULT 10,
    p_result OUT SYS_REFCURSOR
) AS
BEGIN
    OPEN p_result FOR
    SELECT 
        f.FineID,
        f.FineType,
        f.Amount,
        f.PaidAmount,
        f.Amount - f.PaidAmount AS UnpaidAmount,
        f.FineDate,
        f.PayStatus,
        f.Remark,
        br.BorrowRecordID,
        bi.Title AS BookTitle,
        bi.ISBN
    FROM Fine f
    LEFT JOIN BorrowRecord br ON f.BorrowRecordID = br.BorrowRecordID
    LEFT JOIN Book b ON br.BookID = b.BookID
    LEFT JOIN BookInfo bi ON b.ISBN = bi.ISBN
    WHERE f.ReaderID = p_reader_id
    ORDER BY f.FineDate DESC;
END sp_get_reader_fines;
/

-- ================================================================
-- 查询读者待缴罚款汇总
-- ================================================================
CREATE OR REPLACE PROCEDURE sp_get_reader_fine_summary(
    p_reader_id IN INT,
    p_total_fines OUT INT,
    p_unpaid_fines OUT INT,
    p_total_amount OUT DECIMAL,
    p_unpaid_amount OUT DECIMAL
) AS
BEGIN
    SELECT COUNT(*), SUM(Amount), SUM(PaidAmount)
    INTO p_total_fines, p_total_amount, p_unpaid_amount
    FROM Fine
    WHERE ReaderID = p_reader_id;

    SELECT COUNT(*)
    INTO p_unpaid_fines
    FROM Fine
    WHERE ReaderID = p_reader_id
      AND PayStatus IN ('未缴纳', '部分缴纳');
END sp_get_reader_fine_summary;
/
