/* =========================================================
   三人版图书馆管理系统数据库结构脚本
   Database: Oracle 19c
   Scope: 15 core tables only

   保留表：
   1. Reader
   2. Librarian
   3. Building
   4. Bookshelf
   5. BookInfo
   6. Book
   7. Category
   8. Book_Classify
   9. BorrowRecord
   10. Booklist
   11. Booklist_Book
   12. Collect
   13. Comment_Table
   14. Report
   15. Announcement
   ========================================================= */


/* =========================================================
   1. Reader - 读者表
   ========================================================= */
CREATE TABLE Reader (
    ReaderID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    Username VARCHAR2(20) NOT NULL,
    Password VARCHAR2(120) NOT NULL,
    Fullname VARCHAR2(40) NOT NULL,
    Nickname VARCHAR2(40) DEFAULT '默认用户',
    Phone VARCHAR2(20),
    Email VARCHAR2(80),
    CreditScore INT DEFAULT 100 NOT NULL,
    AccountStatus VARCHAR2(10) DEFAULT '正常' NOT NULL,
    Permission VARCHAR2(10) DEFAULT '普通' NOT NULL,
    RegisterTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT uq_reader_username UNIQUE (Username),
    CONSTRAINT chk_reader_credit CHECK (CreditScore BETWEEN 0 AND 100),
    CONSTRAINT chk_reader_status CHECK (AccountStatus IN ('正常', '冻结')),
    CONSTRAINT chk_reader_permission CHECK (Permission IN ('普通', '高级'))
);


/* =========================================================
   2. Librarian - 管理员表
   ========================================================= */
CREATE TABLE Librarian (
    LibrarianID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    StaffNo VARCHAR2(20) NOT NULL,
    Password VARCHAR2(120) NOT NULL,
    Name VARCHAR2(40) NOT NULL,
    Permission VARCHAR2(10) DEFAULT '普通' NOT NULL,
    AccountStatus VARCHAR2(10) DEFAULT '正常' NOT NULL,

    CONSTRAINT uq_librarian_staffno UNIQUE (StaffNo),
    CONSTRAINT chk_librarian_permission CHECK (Permission IN ('普通', '高级')),
    CONSTRAINT chk_librarian_status CHECK (AccountStatus IN ('正常', '停用'))
);


/* =========================================================
   3. Building - 楼宇表
   ========================================================= */
CREATE TABLE Building (
    BuildingID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    BuildingName VARCHAR2(40) NOT NULL,
    Address VARCHAR2(100),
    TotalFloors INT,
    OpenHours VARCHAR2(50),
    LibrarianID INT,
    Remark CLOB,

    CONSTRAINT fk_building_librarian
        FOREIGN KEY (LibrarianID) REFERENCES Librarian(LibrarianID),
    CONSTRAINT chk_building_floors CHECK (TotalFloors IS NULL OR TotalFloors > 0)
);


/* =========================================================
   4. Bookshelf - 书架表
   ========================================================= */
CREATE TABLE Bookshelf (
    ShelfID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    BuildingID INT NOT NULL,
    ShelfCode VARCHAR2(20) NOT NULL,
    Floor INT NOT NULL,
    Zone VARCHAR2(20),

    CONSTRAINT uq_bookshelf_location UNIQUE (BuildingID, ShelfCode),
    CONSTRAINT fk_bookshelf_building
        FOREIGN KEY (BuildingID) REFERENCES Building(BuildingID),
    CONSTRAINT chk_bookshelf_floor CHECK (Floor > 0)
);


/* =========================================================
   5. BookInfo - 图书信息表，ISBN 维度
   ========================================================= */
CREATE TABLE BookInfo (
    ISBN VARCHAR2(20) PRIMARY KEY,
    Title VARCHAR2(100) NOT NULL,
    Author VARCHAR2(80) NOT NULL,
    Publisher VARCHAR2(80),
    PublishYear INT,
    Price DECIMAL(8, 2),
    Summary CLOB,
    TotalStock INT DEFAULT 0 NOT NULL,
    AvailableStock INT DEFAULT 0 NOT NULL,

    CONSTRAINT chk_bookinfo_stock CHECK (
        TotalStock >= 0
        AND AvailableStock >= 0
        AND AvailableStock <= TotalStock
    ),
    CONSTRAINT chk_bookinfo_price CHECK (Price IS NULL OR Price >= 0)
);


/* =========================================================
   6. Book - 实体馆藏图书表
   ========================================================= */
CREATE TABLE Book (
    BookID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    Barcode VARCHAR2(30) NOT NULL,
    ISBN VARCHAR2(20) NOT NULL,
    ShelfID INT,
    Status VARCHAR2(10) DEFAULT '正常' NOT NULL,
    InDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT uq_book_barcode UNIQUE (Barcode),
    CONSTRAINT fk_book_bookinfo
        FOREIGN KEY (ISBN) REFERENCES BookInfo(ISBN),
    CONSTRAINT fk_book_bookshelf
        FOREIGN KEY (ShelfID) REFERENCES Bookshelf(ShelfID),
    CONSTRAINT chk_book_status CHECK (Status IN ('正常', '借出', '下架', '遗失'))
);


/* =========================================================
   7. Category - 图书分类表
   ========================================================= */
CREATE TABLE Category (
    CategoryID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    CategoryName VARCHAR2(40) NOT NULL,
    ParentCategoryID INT,

    CONSTRAINT uq_category_name_parent UNIQUE (CategoryName, ParentCategoryID),
    CONSTRAINT fk_category_parent
        FOREIGN KEY (ParentCategoryID) REFERENCES Category(CategoryID)
);


/* =========================================================
   8. Book_Classify - 图书分类关系表
   ========================================================= */
CREATE TABLE Book_Classify (
    ISBN VARCHAR2(20) NOT NULL,
    CategoryID INT NOT NULL,

    CONSTRAINT pk_book_classify PRIMARY KEY (ISBN, CategoryID),
    CONSTRAINT fk_bc_bookinfo
        FOREIGN KEY (ISBN) REFERENCES BookInfo(ISBN),
    CONSTRAINT fk_bc_category
        FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID)
);


/* =========================================================
   9. BorrowRecord - 借阅记录表
   ========================================================= */
CREATE TABLE BorrowRecord (
    BorrowRecordID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ReaderID INT NOT NULL,
    BookID INT NOT NULL,
    BorrowTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    DueTime TIMESTAMP NOT NULL,
    ReturnTime TIMESTAMP,
    OverdueFine DECIMAL(8, 2) DEFAULT 0 NOT NULL,
    Status VARCHAR2(10) DEFAULT '借阅中' NOT NULL,

    CONSTRAINT fk_borrow_reader
        FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID),
    CONSTRAINT fk_borrow_book
        FOREIGN KEY (BookID) REFERENCES Book(BookID),
    CONSTRAINT chk_borrow_status CHECK (Status IN ('借阅中', '已归还', '逾期', '遗失')),
    CONSTRAINT chk_borrow_fine CHECK (OverdueFine >= 0),
    CONSTRAINT chk_borrow_time CHECK (DueTime > BorrowTime)
);


/* =========================================================
   10. Booklist - 书单表
   ========================================================= */
CREATE TABLE Booklist (
    BooklistID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ListCode VARCHAR2(20) NOT NULL,
    BooklistName VARCHAR2(100) NOT NULL,
    BooklistIntroduction CLOB,
    CreatorID INT NOT NULL,
    CreateTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR2(10) DEFAULT '公开' NOT NULL,

    CONSTRAINT uq_booklist_code UNIQUE (ListCode),
    CONSTRAINT fk_booklist_reader
        FOREIGN KEY (CreatorID) REFERENCES Reader(ReaderID),
    CONSTRAINT chk_booklist_status CHECK (Status IN ('公开', '私有', '删除'))
);


/* =========================================================
   11. Booklist_Book - 书单图书关系表
   ========================================================= */
CREATE TABLE Booklist_Book (
    BooklistID INT NOT NULL,
    ISBN VARCHAR2(20) NOT NULL,
    AddTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Notes CLOB,

    CONSTRAINT pk_booklist_book PRIMARY KEY (BooklistID, ISBN),
    CONSTRAINT fk_booklist_book_booklist
        FOREIGN KEY (BooklistID) REFERENCES Booklist(BooklistID),
    CONSTRAINT fk_booklist_book_bookinfo
        FOREIGN KEY (ISBN) REFERENCES BookInfo(ISBN)
);


/* =========================================================
   12. Collect - 书单收藏表
   ========================================================= */
CREATE TABLE Collect (
    BooklistID INT NOT NULL,
    ReaderID INT NOT NULL,
    FavoriteTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Notes CLOB,

    CONSTRAINT pk_collect PRIMARY KEY (BooklistID, ReaderID),
    CONSTRAINT fk_collect_booklist
        FOREIGN KEY (BooklistID) REFERENCES Booklist(BooklistID),
    CONSTRAINT fk_collect_reader
        FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID)
);


/* =========================================================
   13. Comment_Table - 图书评论表
   ========================================================= */
CREATE TABLE Comment_Table (
    CommentID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ReaderID INT NOT NULL,
    ISBN VARCHAR2(20) NOT NULL,
    Rating INT NOT NULL,
    ReviewContent CLOB,
    CreateTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR2(10) DEFAULT '正常' NOT NULL,

    CONSTRAINT fk_comment_reader
        FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID),
    CONSTRAINT fk_comment_bookinfo
        FOREIGN KEY (ISBN) REFERENCES BookInfo(ISBN),
    CONSTRAINT chk_comment_rating CHECK (Rating BETWEEN 1 AND 5),
    CONSTRAINT chk_comment_status CHECK (Status IN ('正常', '已删除', '屏蔽'))
);


/* =========================================================
   14. Report - 评论举报表
   ========================================================= */
CREATE TABLE Report (
    ReportID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    CommentID INT NOT NULL,
    ReaderID INT NOT NULL,
    ReportReason CLOB NOT NULL,
    ReportTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR2(10) DEFAULT '待处理' NOT NULL,
    LibrarianID INT,
    HandleTime TIMESTAMP,
    HandleResult CLOB,

    CONSTRAINT fk_report_comment
        FOREIGN KEY (CommentID) REFERENCES Comment_Table(CommentID),
    CONSTRAINT fk_report_reader
        FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID),
    CONSTRAINT fk_report_librarian
        FOREIGN KEY (LibrarianID) REFERENCES Librarian(LibrarianID),
    CONSTRAINT chk_report_status CHECK (Status IN ('待处理', '驳回', '处理完成'))
);


/* =========================================================
   15. Announcement - 公告表
   ========================================================= */
CREATE TABLE Announcement (
    AnnouncementID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    LibrarianID INT NOT NULL,
    Title VARCHAR2(100) NOT NULL,
    Content CLOB NOT NULL,
    CreateTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    TargetGroup VARCHAR2(10) DEFAULT '所有人' NOT NULL,
    Status VARCHAR2(10) DEFAULT '发布中' NOT NULL,

    CONSTRAINT fk_announcement_librarian
        FOREIGN KEY (LibrarianID) REFERENCES Librarian(LibrarianID),
    CONSTRAINT chk_announcement_target CHECK (TargetGroup IN ('所有人', '读者', '管理员')),
    CONSTRAINT chk_announcement_status CHECK (Status IN ('发布中', '已撤回'))
);
