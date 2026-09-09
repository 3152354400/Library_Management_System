-- ================================================================
-- 管理员罚单管理视图
-- 用于管理员查询和处理罚款
-- ================================================================
CREATE OR REPLACE VIEW V_AdminFineManagement AS
SELECT
    f.FineID,
    f.ReaderID,
    r.Username,
    r.FullName AS ReaderName,
    r.Phone,
    r.Email,
    f.FineType,
    f.Amount,
    f.PaidAmount,
    f.Amount - f.PaidAmount AS UnpaidAmount,
    f.FineDate,
    f.PayStatus,
    f.DueDate,
    f.Remark,
    bi.Title AS BookTitle,
    bi.ISBN,
    br.BorrowRecordID,
    br.BorrowTime,
    br.DueTime,
    br.ReturnTime
FROM Fine f
LEFT JOIN Reader r ON f.ReaderID = r.ReaderID
LEFT JOIN BorrowRecord br ON f.BorrowRecordID = br.BorrowRecordID
LEFT JOIN Book b ON br.BookID = b.BookID
LEFT JOIN BookInfo bi ON b.ISBN = bi.ISBN;

-- ================================================================
-- 待处理荐购视图
-- 用于管理员查看待处理的荐购申请
-- ================================================================
CREATE OR REPLACE VIEW V_PendingRecommendPurchases AS
SELECT
    rp.RecommendID,
    rp.ReaderID,
    r.Username,
    r.FullName AS ReaderName,
    rp.ISBN,
    rp.Title,
    rp.Author,
    rp.Publisher,
    rp.PublishYear,
    rp.Reason,
    rp.RecommendTime,
    rp.Status
FROM RecommendPurchase rp
JOIN Reader r ON rp.ReaderID = r.ReaderID
WHERE rp.Status = '待审核'
ORDER BY rp.RecommendTime ASC;

-- ================================================================
-- 荐购统计视图
-- 用于统计荐购情况
-- ================================================================
CREATE OR REPLACE VIEW V_RecommendPurchaseStats AS
SELECT
    r.Username,
    r.FullName,
    COUNT(*) AS TotalRecommends,
    COUNT(CASE WHEN rp.Status = '已采纳' THEN 1 END) AS AcceptedCount,
    COUNT(CASE WHEN rp.Status = '已购买' THEN 1 END) AS PurchasedCount,
    COUNT(CASE WHEN rp.Status = '被拒绝' THEN 1 END) AS RejectedCount
FROM RecommendPurchase rp
JOIN Reader r ON rp.ReaderID = r.ReaderID
GROUP BY r.Username, r.FullName;
