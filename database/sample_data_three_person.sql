/* =========================================================
   三人版图书馆管理系统 - 测试数据脚本
   Database: Oracle 19c
   Depends on:
   - schema_three_person.sql
   - triggers_three_person.sql

   说明：
   1. 由于主键使用 GENERATED ALWAYS AS IDENTITY，本脚本不显式插入主键。
   2. 插入顺序严格遵循外键依赖。
   3. BookInfo 的库存字段先置为 0，实体书插入后由触发器自动同步库存。
   4. 本脚本假设测试库为空，适合在新建删减版 Schema 后执行。
   ========================================================= */


/* =========================================================
   1. Librarian - 管理员测试数据
   ========================================================= */
INSERT INTO Librarian (StaffNo, Password, Name, Permission, AccountStatus)
VALUES ('LIB001', 'pwd_hash_001', '张管理员', '高级', '正常');

INSERT INTO Librarian (StaffNo, Password, Name, Permission, AccountStatus)
VALUES ('LIB002', 'pwd_hash_002', '李馆员', '普通', '正常');


/* =========================================================
   2. Reader - 读者测试数据
   ========================================================= */
INSERT INTO Reader (Username, Password, Fullname, Nickname, Phone, Email, CreditScore, AccountStatus, Permission)
VALUES ('reader001', 'pwd_hash_101', '王小明', '小明', '13800000001', 'reader001@example.com', 100, '正常', '普通');

INSERT INTO Reader (Username, Password, Fullname, Nickname, Phone, Email, CreditScore, AccountStatus, Permission)
VALUES ('reader002', 'pwd_hash_102', '赵小红', '小红', '13800000002', 'reader002@example.com', 92, '正常', '普通');

INSERT INTO Reader (Username, Password, Fullname, Nickname, Phone, Email, CreditScore, AccountStatus, Permission)
VALUES ('reader003', 'pwd_hash_103', '陈老师', '陈老师', '13800000003', 'reader003@example.com', 88, '正常', '高级');


/* =========================================================
   3. Building - 楼宇测试数据
   ========================================================= */
INSERT INTO Building (BuildingName, Address, TotalFloors, OpenHours, LibrarianID, Remark)
SELECT '中心图书馆', '校园东区', 6, '08:00-22:00', LibrarianID, '主馆，收藏综合类图书'
FROM Librarian
WHERE StaffNo = 'LIB001';

INSERT INTO Building (BuildingName, Address, TotalFloors, OpenHours, LibrarianID, Remark)
SELECT '理工图书馆', '校园西区', 5, '08:30-21:30', LibrarianID, '理工类专业图书馆'
FROM Librarian
WHERE StaffNo = 'LIB002';


/* =========================================================
   4. Bookshelf - 书架测试数据
   ========================================================= */
INSERT INTO Bookshelf (BuildingID, ShelfCode, Floor, Zone)
SELECT BuildingID, 'A-01', 1, '文学区'
FROM Building
WHERE BuildingName = '中心图书馆';

INSERT INTO Bookshelf (BuildingID, ShelfCode, Floor, Zone)
SELECT BuildingID, 'A-02', 2, '社科区'
FROM Building
WHERE BuildingName = '中心图书馆';

INSERT INTO Bookshelf (BuildingID, ShelfCode, Floor, Zone)
SELECT BuildingID, 'B-01', 1, '计算机区'
FROM Building
WHERE BuildingName = '理工图书馆';

INSERT INTO Bookshelf (BuildingID, ShelfCode, Floor, Zone)
SELECT BuildingID, 'B-02', 2, '工程技术区'
FROM Building
WHERE BuildingName = '理工图书馆';


/* =========================================================
   5. BookInfo - 图书信息测试数据
   ========================================================= */
INSERT INTO BookInfo (ISBN, Title, Author, Publisher, PublishYear, Price, Summary, TotalStock, AvailableStock)
VALUES ('9787111213826', '数据库系统概论', '王珊', '机械工业出版社', 2023, 59.00, '数据库基础理论与应用教材。', 0, 0);

INSERT INTO BookInfo (ISBN, Title, Author, Publisher, PublishYear, Price, Summary, TotalStock, AvailableStock)
VALUES ('9787302423287', '计算机网络', '谢希仁', '清华大学出版社', 2022, 49.80, '计算机网络经典教材。', 0, 0);

INSERT INTO BookInfo (ISBN, Title, Author, Publisher, PublishYear, Price, Summary, TotalStock, AvailableStock)
VALUES ('9787020002207', '红楼梦', '曹雪芹', '人民文学出版社', 2021, 68.00, '中国古典文学名著。', 0, 0);

INSERT INTO BookInfo (ISBN, Title, Author, Publisher, PublishYear, Price, Summary, TotalStock, AvailableStock)
VALUES ('9787115546081', 'Python编程：从入门到实践', 'Eric Matthes', '人民邮电出版社', 2023, 89.00, 'Python 编程入门与实践教程。', 0, 0);

INSERT INTO BookInfo (ISBN, Title, Author, Publisher, PublishYear, Price, Summary, TotalStock, AvailableStock)
VALUES ('9787508647357', '人类简史', '尤瓦尔·赫拉利', '中信出版社', 2020, 58.00, '关于人类历史发展的通俗读物。', 0, 0);


/* =========================================================
   6. Book - 实体馆藏图书测试数据
   说明：插入后会触发 trg_book_after_insert 自动同步库存。
   ========================================================= */
INSERT INTO Book (Barcode, ISBN, ShelfID, Status)
SELECT 'BK000001', '9787111213826', ShelfID, '正常'
FROM Bookshelf
WHERE ShelfCode = 'B-01';

INSERT INTO Book (Barcode, ISBN, ShelfID, Status)
SELECT 'BK000002', '9787111213826', ShelfID, '正常'
FROM Bookshelf
WHERE ShelfCode = 'B-01';

INSERT INTO Book (Barcode, ISBN, ShelfID, Status)
SELECT 'BK000003', '9787302423287', ShelfID, '正常'
FROM Bookshelf
WHERE ShelfCode = 'B-01';

INSERT INTO Book (Barcode, ISBN, ShelfID, Status)
SELECT 'BK000004', '9787302423287', ShelfID, '下架'
FROM Bookshelf
WHERE ShelfCode = 'B-02';

INSERT INTO Book (Barcode, ISBN, ShelfID, Status)
SELECT 'BK000005', '9787020002207', ShelfID, '正常'
FROM Bookshelf
WHERE ShelfCode = 'A-01';

INSERT INTO Book (Barcode, ISBN, ShelfID, Status)
SELECT 'BK000006', '9787020002207', ShelfID, '正常'
FROM Bookshelf
WHERE ShelfCode = 'A-01';

INSERT INTO Book (Barcode, ISBN, ShelfID, Status)
SELECT 'BK000007', '9787115546081', ShelfID, '正常'
FROM Bookshelf
WHERE ShelfCode = 'B-01';

INSERT INTO Book (Barcode, ISBN, ShelfID, Status)
SELECT 'BK000008', '9787115546081', ShelfID, '正常'
FROM Bookshelf
WHERE ShelfCode = 'B-01';

INSERT INTO Book (Barcode, ISBN, ShelfID, Status)
SELECT 'BK000009', '9787508647357', ShelfID, '正常'
FROM Bookshelf
WHERE ShelfCode = 'A-02';

INSERT INTO Book (Barcode, ISBN, ShelfID, Status)
SELECT 'BK000010', '9787508647357', ShelfID, '遗失'
FROM Bookshelf
WHERE ShelfCode = 'A-02';


/* =========================================================
   7. Category - 图书分类测试数据
   ========================================================= */
INSERT INTO Category (CategoryName, ParentCategoryID)
VALUES ('文学', NULL);

INSERT INTO Category (CategoryName, ParentCategoryID)
VALUES ('计算机', NULL);

INSERT INTO Category (CategoryName, ParentCategoryID)
VALUES ('历史', NULL);

INSERT INTO Category (CategoryName, ParentCategoryID)
SELECT '数据库', CategoryID
FROM Category
WHERE CategoryName = '计算机' AND ParentCategoryID IS NULL;

INSERT INTO Category (CategoryName, ParentCategoryID)
SELECT '编程语言', CategoryID
FROM Category
WHERE CategoryName = '计算机' AND ParentCategoryID IS NULL;


/* =========================================================
   8. Book_Classify - 图书分类关系测试数据
   ========================================================= */
INSERT INTO Book_Classify (ISBN, CategoryID)
SELECT '9787111213826', CategoryID
FROM Category
WHERE CategoryName = '数据库';

INSERT INTO Book_Classify (ISBN, CategoryID)
SELECT '9787302423287', CategoryID
FROM Category
WHERE CategoryName = '计算机' AND ParentCategoryID IS NULL;

INSERT INTO Book_Classify (ISBN, CategoryID)
SELECT '9787020002207', CategoryID
FROM Category
WHERE CategoryName = '文学' AND ParentCategoryID IS NULL;

INSERT INTO Book_Classify (ISBN, CategoryID)
SELECT '9787115546081', CategoryID
FROM Category
WHERE CategoryName = '编程语言';

INSERT INTO Book_Classify (ISBN, CategoryID)
SELECT '9787508647357', CategoryID
FROM Category
WHERE CategoryName = '历史' AND ParentCategoryID IS NULL;


/* =========================================================
   9. BorrowRecord - 借阅记录测试数据
   说明：为避免触发器重复扣减库存，这里同时显式设置实体书状态。
   ========================================================= */
INSERT INTO BorrowRecord (ReaderID, BookID, BorrowTime, DueTime, ReturnTime, OverdueFine, Status)
SELECT r.ReaderID,
       b.BookID,
       CURRENT_TIMESTAMP - INTERVAL '10' DAY,
       CURRENT_TIMESTAMP + INTERVAL '20' DAY,
       NULL,
       0,
       '借阅中'
FROM Reader r
JOIN Book b ON b.Barcode = 'BK000001'
WHERE r.Username = 'reader001';

UPDATE Book
SET Status = '借出'
WHERE Barcode = 'BK000001';

INSERT INTO BorrowRecord (ReaderID, BookID, BorrowTime, DueTime, ReturnTime, OverdueFine, Status)
SELECT r.ReaderID,
       b.BookID,
       CURRENT_TIMESTAMP - INTERVAL '40' DAY,
       CURRENT_TIMESTAMP - INTERVAL '10' DAY,
       NULL,
       5.00,
       '逾期'
FROM Reader r
JOIN Book b ON b.Barcode = 'BK000003'
WHERE r.Username = 'reader002';

UPDATE Book
SET Status = '借出'
WHERE Barcode = 'BK000003';

INSERT INTO BorrowRecord (ReaderID, BookID, BorrowTime, DueTime, ReturnTime, OverdueFine, Status)
SELECT r.ReaderID,
       b.BookID,
       CURRENT_TIMESTAMP - INTERVAL '50' DAY,
       CURRENT_TIMESTAMP - INTERVAL '20' DAY,
       CURRENT_TIMESTAMP - INTERVAL '15' DAY,
       2.50,
       '已归还'
FROM Reader r
JOIN Book b ON b.Barcode = 'BK000005'
WHERE r.Username = 'reader003';


/* =========================================================
   10. Booklist - 书单测试数据
   ========================================================= */
INSERT INTO Booklist (ListCode, BooklistName, BooklistIntroduction, CreatorID, Status)
SELECT 'LIST001', '数据库学习书单', '适合数据库课程学习的图书集合。', ReaderID, '公开'
FROM Reader
WHERE Username = 'reader001';

INSERT INTO Booklist (ListCode, BooklistName, BooklistIntroduction, CreatorID, Status)
SELECT 'LIST002', '人文阅读书单', '文学与历史类阅读推荐。', ReaderID, '公开'
FROM Reader
WHERE Username = 'reader002';


/* =========================================================
   11. Booklist_Book - 书单图书关系测试数据
   ========================================================= */
INSERT INTO Booklist_Book (BooklistID, ISBN, Notes)
SELECT BooklistID, '9787111213826', '数据库课程重点教材'
FROM Booklist
WHERE ListCode = 'LIST001';

INSERT INTO Booklist_Book (BooklistID, ISBN, Notes)
SELECT BooklistID, '9787302423287', '计算机基础补充阅读'
FROM Booklist
WHERE ListCode = 'LIST001';

INSERT INTO Booklist_Book (BooklistID, ISBN, Notes)
SELECT BooklistID, '9787115546081', '编程实践入门'
FROM Booklist
WHERE ListCode = 'LIST001';

INSERT INTO Booklist_Book (BooklistID, ISBN, Notes)
SELECT BooklistID, '9787020002207', '古典文学阅读'
FROM Booklist
WHERE ListCode = 'LIST002';

INSERT INTO Booklist_Book (BooklistID, ISBN, Notes)
SELECT BooklistID, '9787508647357', '通识历史阅读'
FROM Booklist
WHERE ListCode = 'LIST002';


/* =========================================================
   12. Collect - 收藏测试数据
   ========================================================= */
INSERT INTO Collect (BooklistID, ReaderID, Notes)
SELECT bl.BooklistID, r.ReaderID, '后续学习数据库时参考'
FROM Booklist bl
JOIN Reader r ON r.Username = 'reader002'
WHERE bl.ListCode = 'LIST001';

INSERT INTO Collect (BooklistID, ReaderID, Notes)
SELECT bl.BooklistID, r.ReaderID, '适合课外阅读'
FROM Booklist bl
JOIN Reader r ON r.Username = 'reader003'
WHERE bl.ListCode = 'LIST002';


/* =========================================================
   13. Comment_Table - 评论测试数据
   ========================================================= */
INSERT INTO Comment_Table (ReaderID, ISBN, Rating, ReviewContent, Status)
SELECT ReaderID, '9787111213826', 5, '内容系统，适合数据库课程学习。', '正常'
FROM Reader
WHERE Username = 'reader001';

INSERT INTO Comment_Table (ReaderID, ISBN, Rating, ReviewContent, Status)
SELECT ReaderID, '9787020002207', 5, '经典文学作品，值得反复阅读。', '正常'
FROM Reader
WHERE Username = 'reader002';

INSERT INTO Comment_Table (ReaderID, ISBN, Rating, ReviewContent, Status)
SELECT ReaderID, '9787302423287', 4, '网络知识讲解清晰，但部分章节较难。', '正常'
FROM Reader
WHERE Username = 'reader003';


/* =========================================================
   14. Report - 举报测试数据
   ========================================================= */
INSERT INTO Report (CommentID, ReaderID, ReportReason, Status)
SELECT cm.CommentID, r.ReaderID, '评论内容疑似与图书无关，需要管理员确认。', '待处理'
FROM Comment_Table cm
JOIN Reader r ON r.Username = 'reader002'
WHERE cm.ISBN = '9787111213826'
  AND cm.ReaderID = (SELECT ReaderID FROM Reader WHERE Username = 'reader001');

INSERT INTO Report (CommentID, ReaderID, ReportReason, Status, LibrarianID, HandleTime, HandleResult)
SELECT cm.CommentID,
       r.ReaderID,
       '举报测试数据，管理员已驳回。',
       '驳回',
       l.LibrarianID,
       CURRENT_TIMESTAMP,
       '评论内容正常，驳回举报。'
FROM Comment_Table cm
JOIN Reader r ON r.Username = 'reader001'
JOIN Librarian l ON l.StaffNo = 'LIB001'
WHERE cm.ISBN = '9787020002207'
  AND cm.ReaderID = (SELECT ReaderID FROM Reader WHERE Username = 'reader002');


/* =========================================================
   15. Announcement - 公告测试数据
   ========================================================= */
INSERT INTO Announcement (LibrarianID, Title, Content, TargetGroup, Status)
SELECT LibrarianID, '图书馆开放时间调整通知', '本周末图书馆开放时间调整为 09:00-18:00。', '所有人', '发布中'
FROM Librarian
WHERE StaffNo = 'LIB001';

INSERT INTO Announcement (LibrarianID, Title, Content, TargetGroup, Status)
SELECT LibrarianID, '逾期归还提醒', '请读者及时归还到期图书，避免产生逾期罚款并影响信用分。', '读者', '发布中'
FROM Librarian
WHERE StaffNo = 'LIB002';

COMMIT;
