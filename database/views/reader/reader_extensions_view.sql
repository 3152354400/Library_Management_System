-- ================================================================
-- 读者续借记录视图
-- 用于查询读者的续借历史
-- ================================================================
CREATE OR REPLACE VIEW V_ReaderRenewRecords AS
SELECT
    rr.RenewID,
    rr.ReaderID,
    r.Username,
    r.FullName AS ReaderName,
    rr.BorrowRecordID,
    bi.Title AS BookTitle,
    bi.ISBN,
    rr.RenewTime,
    rr.OldDueTime,
    rr.NewDueTime,
    rr.RenewCount,
    rr.Status AS RenewStatus
FROM RenewRecord rr
JOIN Reader r ON rr.ReaderID = r.ReaderID
JOIN BorrowRecord br ON rr.BorrowRecordID = br.BorrowRecordID
JOIN Book b ON br.BookID = b.BookID
JOIN BookInfo bi ON b.ISBN = bi.ISBN;

-- ================================================================
-- 读者罚款汇总视图
-- 用于查询读者的罚款汇总信息
-- ================================================================
CREATE OR REPLACE VIEW V_ReaderFineSummary AS
SELECT
    r.ReaderID,
    r.Username,
    r.FullName AS ReaderName,
    COUNT(f.FineID) AS TotalFines,
    SUM(f.Amount) AS TotalAmount,
    SUM(f.PaidAmount) AS TotalPaid,
    SUM(f.Amount - f.PaidAmount) AS UnpaidAmount,
    COUNT(CASE WHEN f.PayStatus IN ('未缴纳', '部分缴纳') THEN 1 END) AS UnpaidCount,
    MAX(f.FineDate) AS LastFineDate
FROM Reader r
LEFT JOIN Fine f ON r.ReaderID = f.ReaderID
GROUP BY r.ReaderID, r.Username, r.FullName;

-- ================================================================
-- 罚款类型统计视图
-- 用于统计分析各类罚款情况
-- ================================================================
CREATE OR REPLACE VIEW V_FineStatistics AS
SELECT
    FineType,
    COUNT(*) AS FineCount,
    SUM(Amount) AS TotalAmount,
    SUM(PaidAmount) AS TotalPaid,
    SUM(Amount - PaidAmount) AS UnpaidAmount,
    AVG(Amount) AS AvgAmount,
    MAX(Amount) AS MaxAmount
FROM Fine
GROUP BY FineType;

-- ================================================================
-- 预约统计视图
-- 用于查询各图书的预约热度
-- ================================================================
CREATE OR REPLACE VIEW V_BookReservationStats AS
SELECT
    bi.ISBN,
    bi.Title,
    bi.Author,
    bi.AvailableStock,
    COUNT(rb.ReserveID) AS TotalReservations,
    COUNT(CASE WHEN rb.Status = '等待中' THEN 1 END) AS WaitingCount,
    COUNT(CASE WHEN rb.Status = '已通知' THEN 1 END) AS NotifiedCount,
    COUNT(CASE WHEN rb.Status = '已借出' THEN 1 END) AS BorrowedCount,
    MIN(rb.ReserveTime) AS FirstReserveDate,
    MAX(rb.ReserveTime) AS LatestReserveDate
FROM BookInfo bi
LEFT JOIN ReserveBook rb ON bi.ISBN = rb.ISBN
GROUP BY bi.ISBN, bi.Title, bi.Author, bi.AvailableStock;

-- ================================================================
-- 操作日志统计视图
-- 用于查询系统操作统计
-- ================================================================
CREATE OR REPLACE VIEW V_OperationLogStats AS
SELECT
    OperationModule,
    OperationAction,
    OperatorType,
    COUNT(*) AS OperationCount,
    SUM(CASE WHEN Status = '成功' THEN 1 ELSE 0 END) AS SuccessCount,
    SUM(CASE WHEN Status = '失败' THEN 1 ELSE 0 END) AS FailCount,
    MAX(OperationTime) AS LastOperationTime
FROM OperationLog
WHERE OperationTime > SYSDATE - 30
GROUP BY OperationModule, OperationAction, OperatorType;

-- ================================================================
-- 读者积分等级视图
-- 用于查询读者信用等级
-- ================================================================
CREATE OR REPLACE VIEW V_ReaderCreditLevel AS
SELECT
    r.ReaderID,
    r.Username,
    r.FullName,
    r.CreditScore,
    CASE
        WHEN r.CreditScore >= 90 THEN 'AAA'
        WHEN r.CreditScore >= 80 THEN 'AA'
        WHEN r.CreditScore >= 70 THEN 'A'
        WHEN r.CreditScore >= 60 THEN 'B'
        WHEN r.CreditScore >= 50 THEN 'C'
        ELSE 'D'
    END AS CreditLevel,
    CASE
        WHEN r.AccountStatus = '正常' AND r.CreditScore >= 60 THEN '正常'
        WHEN r.AccountStatus = '正常' AND r.CreditScore < 60 THEN '信用受限'
        ELSE '账户冻结'
    END AS AccountStatus,
    NVL(r.OverdueCount, 0) AS OverdueCount
FROM Reader r;
