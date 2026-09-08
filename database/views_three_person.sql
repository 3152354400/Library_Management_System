/* =========================================================
   三人版图书馆管理系统 - 视图脚本
   Database: Oracle 19c
   Depends on: schema_three_person.sql
   ========================================================= */


/* =========================================================
   1. V_BookDetail
   功能：图书详情视图。
   内容：图书基本信息、库存、分类、评论数量、平均评分。
   ========================================================= */
CREATE OR REPLACE VIEW V_BookDetail AS
SELECT
    bi.ISBN,
    bi.Title,
    bi.Author,
    bi.Publisher,
    bi.PublishYear,
    bi.Price,
    bi.Summary,
    bi.TotalStock,
    bi.AvailableStock,
    LISTAGG(DISTINCT c.CategoryName, ', ') WITHIN GROUP (ORDER BY c.CategoryName) AS CategoryNames,
    COUNT(DISTINCT cm.CommentID) AS CommentCount,
    ROUND(AVG(CASE WHEN cm.Status = '正常' THEN cm.Rating END), 2) AS AverageRating
FROM BookInfo bi
LEFT JOIN Book_Classify bc ON bi.ISBN = bc.ISBN
LEFT JOIN Category c ON bc.CategoryID = c.CategoryID
LEFT JOIN Comment_Table cm ON bi.ISBN = cm.ISBN AND cm.Status = '正常'
GROUP BY
    bi.ISBN,
    bi.Title,
    bi.Author,
    bi.Publisher,
    bi.PublishYear,
    bi.Price,
    bi.Summary,
    bi.TotalStock,
    bi.AvailableStock;


/* =========================================================
   2. V_ReaderBorrowDetail
   功能：读者借阅详情视图。
   内容：读者信息、实体书信息、图书信息、借阅状态、罚款。
   ========================================================= */
CREATE OR REPLACE VIEW V_ReaderBorrowDetail AS
SELECT
    br.BorrowRecordID,
    r.ReaderID,
    r.Username,
    r.Fullname,
    r.CreditScore,
    r.AccountStatus,
    b.BookID,
    b.Barcode,
    b.Status AS BookStatus,
    bi.ISBN,
    bi.Title,
    bi.Author,
    bs.ShelfID,
    bs.ShelfCode,
    bs.Floor,
    bs.Zone,
    bd.BuildingID,
    bd.BuildingName,
    br.BorrowTime,
    br.DueTime,
    br.ReturnTime,
    br.OverdueFine,
    br.Status AS BorrowStatus
FROM BorrowRecord br
JOIN Reader r ON br.ReaderID = r.ReaderID
JOIN Book b ON br.BookID = b.BookID
JOIN BookInfo bi ON b.ISBN = bi.ISBN
LEFT JOIN Bookshelf bs ON b.ShelfID = bs.ShelfID
LEFT JOIN Building bd ON bs.BuildingID = bd.BuildingID;


/* =========================================================
   3. V_PendingReport
   功能：待处理举报视图。
   内容：举报信息、被举报评论、举报人、图书、处理状态。
   ========================================================= */
CREATE OR REPLACE VIEW V_PendingReport AS
SELECT
    rp.ReportID,
    rp.ReportTime,
    rp.ReportReason,
    rp.Status AS ReportStatus,
    cm.CommentID,
    cm.ReviewContent,
    cm.Rating,
    cm.CreateTime AS CommentTime,
    cm.Status AS CommentStatus,
    r.ReaderID AS ReporterID,
    r.Username AS ReporterUsername,
    bi.ISBN,
    bi.Title AS BookTitle
FROM Report rp
JOIN Comment_Table cm ON rp.CommentID = cm.CommentID
JOIN Reader r ON rp.ReaderID = r.ReaderID
JOIN BookInfo bi ON cm.ISBN = bi.ISBN
WHERE rp.Status = '待处理';
