/* =========================================================
   补齐 15 表方案缺失的对象（V_BookAdmin_Info / Fine / BookLossReport / BookFavorite / Log）
   依据后端代码 SQL 反推结构，与 dev 分支代码匹配
   ========================================================= */

/* 1. 管理员图书列表视图 */
CREATE OR REPLACE VIEW V_BookAdmin_Info AS
SELECT
    bi.ISBN,
    bi.Title,
    bi.Author,
    bi.TotalStock AS TotalCopies,
    COUNT(b.BookID) AS PhysicalCopies,
    SUM(CASE WHEN b.Status = '正常' THEN 1 ELSE 0 END) AS AvailableCopies,
    SUM(CASE WHEN b.Status = '借出' THEN 1 ELSE 0 END) AS BorrowedCopies,
    SUM(CASE WHEN b.Status IN ('下架', '遗失') THEN 1 ELSE 0 END) AS TakedownCopies
FROM BookInfo bi
LEFT JOIN Book b ON bi.ISBN = b.ISBN
GROUP BY bi.ISBN, bi.Title, bi.Author, bi.TotalStock;

/* 2. 罚款表 */
CREATE TABLE Fine (
    FineID          NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    BorrowRecordID  NUMBER,
    ReaderID        NUMBER,
    Amount          NUMBER(10,2) DEFAULT 0,
    PaidAmount      NUMBER(10,2) DEFAULT 0,
    PayStatus       VARCHAR2(20),
    FineDate        TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    HandlerID       NUMBER,
    Remark          VARCHAR2(500)
);

/* 3. 丢书报备表 */
CREATE TABLE BookLossReport (
    ReportID            NUMBER PRIMARY KEY,
    ReaderID            NUMBER,
    BookID              NUMBER,
    ReportType          VARCHAR2(20),
    ReportTime          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Description         VARCHAR2(500),
    EstimatedValue      NUMBER(10,2),
    Status              VARCHAR2(20) DEFAULT '待处理',
    CompensationAmount  NUMBER(10,2),
    HandleTime          TIMESTAMP,
    HandlerID           NUMBER,
    HandleResult        VARCHAR2(500),
    PaymentStatus       VARCHAR2(20),
    PaymentTime         TIMESTAMP,
    Remark              VARCHAR2(500)
);

/* 4. 图书收藏表 */
CREATE TABLE BookFavorite (
    FavoriteID   NUMBER PRIMARY KEY,
    ReaderID     NUMBER,
    ISBN         VARCHAR2(20),
    Notes        VARCHAR2(500),
    FolderName   VARCHAR2(50) DEFAULT '默认收藏夹',
    FavoriteTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

/* 5. 操作日志表 + 序列 */
CREATE SEQUENCE LOG_SEQ START WITH 1 INCREMENT BY 1;
CREATE TABLE Log (
    LogID            NUMBER PRIMARY KEY,
    OperationTime    TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    OperationContent VARCHAR2(1000),
    OperatorType     VARCHAR2(20),
    OperatorID       VARCHAR2(50),
    OperationStatus  VARCHAR2(20),
    ErrorMessage     VARCHAR2(1000)
);

COMMIT;
