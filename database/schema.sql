-- =================================================================
-- I. 基础表 (无或仅有对已创建表的外键依赖)
-- =================================================================

-- 1. Reader 表 (读者表)
-- 存储读者的基本信息。
CREATE TABLE Reader (
    ReaderID VARCHAR2(20),
    Password VARCHAR2(120) NOT NULL,
    Name VARCHAR2(40) NOT NULL,
    CreditScore INT,
    ReaderType VARCHAR2(10) NOT NULL,
    AccountStatus VARCHAR2(10) NOT NULL,
    Permission VARCHAR2(10) DEFAULT '普通',
    CONSTRAINT pk_reader PRIMARY KEY (ReaderID),
    CONSTRAINT chk_reader_credit CHECK (CreditScore >= 0 AND CreditScore <= 100),
    CONSTRAINT chk_reader_type CHECK (ReaderType IN ('学生', '教师')),
    CONSTRAINT chk_reader_status CHECK (AccountStatus IN ('正常', '冻结')),
    CONSTRAINT chk_reader_permission CHECK (Permission IN ('普通', '高级'))
);

-- 2. BookInfo 表 (图书信息表)
-- 存储图书的通用信息 (ISBN、书名、作者等)。
CREATE TABLE BookInfo (
    ISBN VARCHAR2(20),
    Title VARCHAR2(100) NOT NULL,
    Author VARCHAR2(40) NOT NULL,
    Stock NUMBER(3) NOT NULL,
    CONSTRAINT pk_bookinfo PRIMARY KEY (ISBN)
);

-- 3. Librarian 表 (管理员表)
-- 存储管理员的账户信息。
CREATE TABLE Librarian (
    LibrarianID VARCHAR2(20),
    Password VARCHAR2(120) NOT NULL,
    Name VARCHAR2(40) NOT NULL,
    Permission VARCHAR2(10) NOT NULL,
    CONSTRAINT pk_librarian PRIMARY KEY (LibrarianID),
    CONSTRAINT chk_librarian_permission CHECK (Permission IN ('普通', '高级'))
);

-- 4. Category 表 (分类表)
-- 存储图书的分类信息，支持树状结构。
CREATE TABLE Category (
    CategoryID VARCHAR2(20),
    CategoryName VARCHAR2(40) NOT NULL,
    ParentCategoryID VARCHAR2(20),
    CONSTRAINT pk_category PRIMARY KEY (CategoryID),
    CONSTRAINT fk_category_parent FOREIGN KEY (ParentCategoryID) REFERENCES Category(CategoryID) -- 外键，关联到自身，形成父子分类关系。
);

-- 5. Building 表 (大楼表)
-- 存储图书馆大楼的基本信息。
CREATE TABLE Building (
    BuildingID VARCHAR2(20),
    BuildingName VARCHAR2(40) NOT NULL,
    Address VARCHAR2(100),
    TotalFloors INT,
    OpenHours VARCHAR2(50),
    ManagerID VARCHAR2(20),
    Remark CLOB,
    CONSTRAINT pk_building PRIMARY KEY (BuildingID),
    CONSTRAINT fk_building_manager FOREIGN KEY (ManagerID) REFERENCES Librarian(LibrarianID) -- 外键，关联到 Librarian 表，表示大楼的管理员。
);

-- 6. Bookshelf 表 (书架表)
-- 存储书架的位置信息。
CREATE TABLE Bookshelf (
    BuildingID VARCHAR2(20),
    ShelfCode VARCHAR2(20),
    Floor INT NOT NULL,
    Zone VARCHAR2(20),
    CONSTRAINT pk_bookshelf PRIMARY KEY (BuildingID, ShelfCode),
    CONSTRAINT fk_bookshelf_building FOREIGN KEY (BuildingID) REFERENCES Building(BuildingID) -- 外键，关联到 Building 表，表示书架所属大楼。
);

-- 7. Seat 表 (座位表)
-- 存储座位的详细信息。
CREATE TABLE Seat (
    BuildingID VARCHAR2(20),
    Floor INT,
    SeatNumber VARCHAR2(20),
    Zone VARCHAR2(20),
    ReservationStatus VARCHAR2(10) NOT NULL,
    CONSTRAINT pk_seat PRIMARY KEY (BuildingID, Floor, SeatNumber),
    CONSTRAINT fk_seat_building FOREIGN KEY (BuildingID) REFERENCES Building(BuildingID), -- 外键，关联到 Building 表，表示座位所属大楼。
    CONSTRAINT chk_seat_status CHECK (ReservationStatus IN ('空闲', '已预约'))
);

-- 8. StudyRoom 表 (自习室表)
-- 存储自习室的详细信息。
CREATE TABLE StudyRoom (
    BuildingID VARCHAR2(20),
    Floor INT,
    RoomNumber VARCHAR2(20),
    Zone VARCHAR2(20),
    Capacity INT NOT NULL,
    ReservationStatus VARCHAR2(10) NOT NULL,
    CONSTRAINT pk_studyroom PRIMARY KEY (BuildingID, Floor, RoomNumber),
    CONSTRAINT fk_studyroom_building FOREIGN KEY (BuildingID) REFERENCES Building(BuildingID), -- 外键，关联到 Building 表，表示自习室所属大楼。
    CONSTRAINT chk_studyroom_status CHECK (ReservationStatus IN ('空闲', '已预约'))
);


-- =================================================================
-- II. 核心业务表 (依赖于基础表)
-- =================================================================

-- 9. Book 表 (实体书表)
-- 存储每一本实体书的状态和位置。
CREATE TABLE Book (
    BookID VARCHAR2(20),
    Status VARCHAR2(10) NOT NULL,
    ShelfID VARCHAR2(20),
    BuildingID VARCHAR2(20),
    ISBN VARCHAR2(20) NOT NULL, -- 添加了 ISBN 字段以关联到 BookInfo
    CONSTRAINT pk_book PRIMARY KEY (BookID),
    CONSTRAINT fk_book_info FOREIGN KEY (ISBN) REFERENCES BookInfo(ISBN), -- 外键，关联到 BookInfo 表，表示这本书的通用信息。
    CONSTRAINT fk_book_bookshelf FOREIGN KEY (BuildingID, ShelfID) REFERENCES Bookshelf(BuildingID, ShelfCode), -- 复合外键，关联到 Bookshelf 表，表示实体书所在的书架。
    CONSTRAINT chk_book_status CHECK (Status IN ('正常', '下架', '借出'))
);


-- 10. Booklist 表 (书单表)
-- 存储用户创建的书单。
CREATE TABLE Booklist (
    BooklistID VARCHAR2(20),
    BooklistName VARCHAR2(100) NOT NULL,
    BooklistIntroduction CLOB,
    CreatorID VARCHAR2(20),
    CONSTRAINT pk_booklist PRIMARY KEY (BooklistID),
    CONSTRAINT fk_booklist_creator FOREIGN KEY (CreatorID) REFERENCES Reader(ReaderID) -- 外键，关联到 Reader 表，表示书单的创建者。
);

-- 11. Comment_Table 表 (评论表)
-- 存储读者对图书的评论和评分。
CREATE TABLE Comment_Table (
    CommentID INT,
    ReaderID VARCHAR2(20),
    ISBN VARCHAR2(20),
    Rating INT,
    ReviewContent CLOB,
    CreateTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR2(10) DEFAULT '正常',
    CONSTRAINT pk_comment PRIMARY KEY (CommentID),
    CONSTRAINT fk_comment_reader FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID), -- 外键，关联到 Reader 表，表示评论的发布者。
    CONSTRAINT fk_comment_bookinfo FOREIGN KEY (ISBN) REFERENCES BookInfo(ISBN), -- 外键，关联到 BookInfo 表，表示评论针对的图书。
    CONSTRAINT chk_comment_rating CHECK (Rating >= 1 AND Rating <= 5),
    CONSTRAINT chk_comment_status CHECK (Status IN ('正常', '已删除'))
);

-- 12. Announcement 表 (公告表)
-- 存储由管理员发布的公告。
CREATE TABLE Announcement (
    AnnouncementID INT,
    LibrarianID VARCHAR2(20),
    Title VARCHAR2(100) NOT NULL,
    Content CLOB NOT NULL,
    CreateTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    TargetGroup VARCHAR2(10) NOT NULL,
    Status VARCHAR2(10) NOT NULL,
    CONSTRAINT pk_announcement PRIMARY KEY (AnnouncementID),
    CONSTRAINT fk_announcement_librarian FOREIGN KEY (LibrarianID) REFERENCES Librarian(LibrarianID), -- 外键，关联到 Librarian 表，表示公告的发布者。
    CONSTRAINT chk_announcement_target CHECK (TargetGroup IN ('所有人', '读者', '管理员')),
    CONSTRAINT chk_announcement_status CHECK (Status IN ('发布中', '已撤回'))
);

-- 13. Log 表 (日志表)
-- 记录系统中的重要操作。
CREATE TABLE Log (
    LogID INT,
    OperationTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    OperationContent CLOB NOT NULL,
    OperatorType VARCHAR2(10) NOT NULL,
    OperatorID VARCHAR2(20),
    OperationStatus VARCHAR2(10) NOT NULL,
    ErrorMessage CLOB,
    CONSTRAINT pk_log PRIMARY KEY (LogID),
    CONSTRAINT chk_log_optype CHECK (OperatorType IN ('Reader', 'Librarian')),
    CONSTRAINT chk_log_opstatus CHECK (OperationStatus IN ('成功', '失败'))
);

-- =================================================================
-- III. 关系与记录表 (连接多个表，形成多对多关系或记录)
-- =================================================================

-- 14. BorrowRecord 表 (借阅记录表)
-- 记录图书的借阅历史。
CREATE TABLE BorrowRecord (
    BorrowRecordID INT,
    ReaderID VARCHAR2(20),
    BookID VARCHAR2(20),
    BorrowTime TIMESTAMP NOT NULL,
    ReturnTime TIMESTAMP,
    OverdueFine DECIMAL(5,2) DEFAULT 0,
    CONSTRAINT pk_borrowrecord PRIMARY KEY (BorrowRecordID),
    CONSTRAINT fk_borrow_reader FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID), -- 外键，关联到 Reader 表，表示借阅人。
    CONSTRAINT fk_borrow_book FOREIGN KEY (BookID) REFERENCES Book(BookID) -- 外键，关联到 Book 表，表示被借阅的实体书。
);

-- 15. Reserve_Room 表 (自习室预约记录表)
CREATE TABLE Reserve_Room (
    ReservationID INT,
    ReaderID VARCHAR2(20),
    BuildingID VARCHAR2(20),
    Floor INT,
    RoomNumber VARCHAR2(20),
    ReservationTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    StartTime TIMESTAMP NOT NULL,
    EndTime TIMESTAMP NOT NULL,
    Status VARCHAR2(10) NOT NULL,
    CONSTRAINT pk_reserveroom PRIMARY KEY (ReservationID),
    CONSTRAINT fk_reserveroom_reader FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID), -- 外键，关联到 Reader 表，表示预约人。
    CONSTRAINT fk_reserveroom_room FOREIGN KEY (BuildingID, Floor, RoomNumber) REFERENCES StudyRoom(BuildingID, Floor, RoomNumber), -- 复合外键，关联到 StudyRoom 表，表示被预约的自习室。
    CONSTRAINT chk_reserveroom_status CHECK (Status IN ('已完成', '未完成', '取消'))
);

-- 16. Reserve_Seat 表 (座位预约记录表)
CREATE TABLE Reserve_Seat (
    ReservationID INT,
    ReaderID VARCHAR2(20),
    BuildingID VARCHAR2(20),
    Floor INT,
    SeatNumber VARCHAR2(20),
    ReservationTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    StartTime TIMESTAMP NOT NULL,
    EndTime TIMESTAMP NOT NULL,
    Status VARCHAR2(10) NOT NULL,
    CONSTRAINT pk_reserveseat PRIMARY KEY (ReservationID),
    CONSTRAINT fk_reserveseat_reader FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID), -- 外键，关联到 Reader 表，表示预约人。
    CONSTRAINT fk_reserveseat_seat FOREIGN KEY (BuildingID, Floor, SeatNumber) REFERENCES Seat(BuildingID, Floor, SeatNumber), -- 复合外键，关联到 Seat 表，表示被预约的座位。
    CONSTRAINT chk_reserveseat_status CHECK (Status IN ('已完成', '未完成', '取消'))
);

-- 17. Report 表 (举报记录表)
CREATE TABLE Report (
    ReportID INT,
    CommentID INT,
    ReaderID VARCHAR2(20),
    ReportReason CLOB,
    ReportTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR2(10) NOT NULL,
    LibrarianID VARCHAR2(20),
    CONSTRAINT pk_report PRIMARY KEY (ReportID),
    CONSTRAINT fk_report_comment FOREIGN KEY (CommentID) REFERENCES Comment_Table(CommentID), -- 外键，关联到 Comment_Table 表，表示被举报的评论。
    CONSTRAINT fk_report_reader FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID), -- 外键，关联到 Reader 表，表示举报人。
    CONSTRAINT fk_report_librarian FOREIGN KEY (LibrarianID) REFERENCES Librarian(LibrarianID), -- 外键，关联到 Librarian 表，表示处理举报的管理员。
    CONSTRAINT chk_report_status CHECK (Status IN ('待处理', '驳回', '处理完成'))
);

-- 18. Booklist_Book 表 (书单-图书关联表)
-- 实现书单与图书之间的多对多关系。
CREATE TABLE Booklist_Book (
    BooklistID VARCHAR2(20),
    ISBN VARCHAR2(20),
    AddTime TIMESTAMP,
    Notes CLOB,
    CONSTRAINT pk_booklist_book PRIMARY KEY (BooklistID, ISBN),
    CONSTRAINT fk_bb_booklist FOREIGN KEY (BooklistID) REFERENCES Booklist(BooklistID), -- 外键，关联到 Booklist 表。
    CONSTRAINT fk_bb_bookinfo FOREIGN KEY (ISBN) REFERENCES BookInfo(ISBN) -- 外键，关联到 BookInfo 表。
);

-- 19. Book_Classify 表 (图书-分类关联表)
-- 实现图书与分类之间的多对多关系。
CREATE TABLE Book_Classify (
    ISBN VARCHAR2(20),
    CategoryID VARCHAR2(20),
    RelationNote CLOB,
    CONSTRAINT pk_book_classify PRIMARY KEY (ISBN, CategoryID),
    CONSTRAINT fk_bc_bookinfo FOREIGN KEY (ISBN) REFERENCES BookInfo(ISBN), -- 外键，关联到 BookInfo 表。
    CONSTRAINT fk_bc_category FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID) -- 外键，关联到 Category 表。
);

-- 20. Collect 表 (书单收藏表)
-- 记录读者对书单的收藏行为。
CREATE TABLE Collect (
    BooklistID VARCHAR2(20),
    ReaderID VARCHAR2(20),
    FavoriteTime TIMESTAMP,
    Notes CLOB,
    CONSTRAINT pk_collect PRIMARY KEY (BooklistID, ReaderID),
    CONSTRAINT fk_collect_booklist FOREIGN KEY (BooklistID) REFERENCES Booklist(BooklistID), -- 外键，关联到 Booklist 表。
    CONSTRAINT fk_collect_reader FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID) -- 外键，关联到 Reader 表。
);