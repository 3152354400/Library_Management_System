/* ================================================================
   I. 基础表
   ================================================================ */

/************************************
* 1. Reader – 读者信息表
************************************/
CREATE TABLE Reader (
    ReaderID   INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, -- 读者主键，自增 ID
    Username   VARCHAR2(8)  NOT NULL,                       -- 登录账号/学号，唯一
    Password   VARCHAR2(120) NOT NULL,                       -- 加密后的登录密码 (BCrypt)
    Fullname   VARCHAR2(40)  DEFAULT '',                     -- 真实姓名
    Nickname   VARCHAR2(40)  DEFAULT '默认用户名',            -- 昵称
    Avatar     VARCHAR2(200) DEFAULT '',                     -- 头像 URL
    CreditScore   INT DEFAULT 100,                           -- 信用积分 0‑100
    AccountStatus VARCHAR2(10) DEFAULT '正常',               -- 账户状态：正常/冻结
    Permission    VARCHAR2(10) DEFAULT '普通',               -- 权限等级：普通/高级
    CONSTRAINT uq_reader_username UNIQUE (Username),
    CONSTRAINT chk_reader_credit CHECK (CreditScore BETWEEN 0 AND 100),
    CONSTRAINT chk_reader_status CHECK (AccountStatus IN ('正常','冻结')),
    CONSTRAINT chk_reader_permission CHECK (Permission IN ('普通','高级'))
);
COMMENT ON TABLE  Reader IS '读者信息表';
COMMENT ON COLUMN Reader.ReaderID      IS '读者主键，自增 ID';
COMMENT ON COLUMN Reader.Username      IS '登录账号/学号，唯一';
COMMENT ON COLUMN Reader.Password      IS '加密后的登录密码 (BCrypt)';
COMMENT ON COLUMN Reader.Fullname      IS '真实姓名';
COMMENT ON COLUMN Reader.Nickname      IS '用户昵称';
COMMENT ON COLUMN Reader.Avatar        IS '头像 URL';
COMMENT ON COLUMN Reader.CreditScore   IS '信用积分，范围 0‑100';
COMMENT ON COLUMN Reader.AccountStatus IS '账户状态：正常 / 冻结';
COMMENT ON COLUMN Reader.Permission    IS '权限等级：普通 / 高级';

/************************************
* 2. Librarian – 管理员信息表
************************************/
CREATE TABLE Librarian (
    LibrarianID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, -- 管理员主键，自增 ID
    StaffNo     VARCHAR2(20) NOT NULL,                        -- 员工号/登录账号
    Password    VARCHAR2(120) NOT NULL,                       -- 加密后的登录密码
    Name        VARCHAR2(40)  NOT NULL,                       -- 姓名
    Permission  VARCHAR2(10)  NOT NULL,                       -- 权限等级
    CONSTRAINT uq_librarian_staff UNIQUE (StaffNo),
    CONSTRAINT chk_librarian_permission CHECK (Permission IN ('普通','高级'))
);
COMMENT ON TABLE Librarian IS '图书馆管理员信息表';
COMMENT ON COLUMN Librarian.LibrarianID IS '管理员主键，自增 ID';
COMMENT ON COLUMN Librarian.StaffNo     IS '工号/登录账号，唯一';
COMMENT ON COLUMN Librarian.Password    IS '加密后的登录密码';
COMMENT ON COLUMN Librarian.Name        IS '管理员姓名';
COMMENT ON COLUMN Librarian.Permission  IS '权限等级：普通 / 高级';

/************************************
* 3. Category – 图书分类表 (树形)
************************************/
CREATE TABLE Category (
    CategoryID        VARCHAR2(20) PRIMARY KEY,           -- 分类主键，管理员手动输入
    CategoryName      VARCHAR2(40) NOT NULL,              -- 分类名称
    ParentCategoryID  VARCHAR2(20),                       -- 父级分类 ID，可空
    CONSTRAINT fk_category_parent FOREIGN KEY (ParentCategoryID)
        REFERENCES Category(CategoryID)
);
COMMENT ON TABLE Category IS '图书分类表 (支持多级树结构)';
COMMENT ON COLUMN Category.CategoryID       IS '分类主键，管理员手动输入';
COMMENT ON COLUMN Category.CategoryName     IS '分类名称';
COMMENT ON COLUMN Category.ParentCategoryID IS '父级分类 ID（顶级为空）';

/************************************
* 4. BookInfo – 图书信息 (ISBN 维度)
************************************/
CREATE TABLE BookInfo (
    ISBN   VARCHAR2(20),                           -- 国际标准书号 (主键)
    Title  VARCHAR2(100) NOT NULL,                 -- 书名
    Author VARCHAR2(40)  NOT NULL,                 -- 作者
    Stock  NUMBER(3)     NOT NULL,                 -- 馆藏数量
    CONSTRAINT pk_bookinfo PRIMARY KEY (ISBN)
);
COMMENT ON TABLE BookInfo IS '图书基本信息表 (ISBN 维度)';
COMMENT ON COLUMN BookInfo.ISBN   IS '国际标准书号 (10 或 13 位)';
COMMENT ON COLUMN BookInfo.Title  IS '图书标题';
COMMENT ON COLUMN BookInfo.Author IS '作者';
COMMENT ON COLUMN BookInfo.Stock  IS '馆藏总册数';

/************************************
* 5. Building – 楼宇表
************************************/
CREATE TABLE Building (
    BuildingID   INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, -- 楼宇主键，自增
    BuildingName VARCHAR2(40) NOT NULL,                        -- 楼宇名称
    Address      VARCHAR2(100),                                -- 具体地址
    TotalFloors  INT,                                          -- 总楼层数
    OpenHours    VARCHAR2(50),                                 -- 开放时间段
    LibrarianID  INT,                                          -- 负责人 ID
    Remark       CLOB,                                         -- 备注
    CONSTRAINT fk_building_manager FOREIGN KEY (LibrarianID)
        REFERENCES Librarian(LibrarianID)
);
COMMENT ON TABLE Building IS '图书馆楼宇/校区表';
COMMENT ON COLUMN Building.BuildingID   IS '楼宇主键，自增 ID';
COMMENT ON COLUMN Building.BuildingName IS '楼宇名称';
COMMENT ON COLUMN Building.Address      IS '楼宇地址';
COMMENT ON COLUMN Building.TotalFloors  IS '总楼层数';
COMMENT ON COLUMN Building.OpenHours    IS '开放时间段';
COMMENT ON COLUMN Building.LibrarianID  IS '楼宇负责人 (管理员) ID';
COMMENT ON COLUMN Building.Remark       IS '备注';

/************************************
* 6. Bookshelf – 书架表
************************************/
CREATE TABLE Bookshelf (
    ShelfID    INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, -- 主键，自增
    BuildingID INT NOT NULL,                                 -- 所属楼宇 ID
    ShelfCode  VARCHAR2(20) NOT NULL,                        -- 书架编码
    Floor      INT NOT NULL,                                 -- 楼层
    Zone       VARCHAR2(20),                                 -- 区域
    CONSTRAINT uq_bookshelf_loc UNIQUE (BuildingID, ShelfCode),
    CONSTRAINT fk_bookshelf_building FOREIGN KEY (BuildingID)
        REFERENCES Building(BuildingID)
);
COMMENT ON TABLE Bookshelf IS '书架信息表';
COMMENT ON COLUMN Bookshelf.ShelfID    IS '书架主键，自增 ID';
COMMENT ON COLUMN Bookshelf.BuildingID IS '所属楼宇 ID';
COMMENT ON COLUMN Bookshelf.ShelfCode  IS '书架编码 (业务标识)';
COMMENT ON COLUMN Bookshelf.Floor      IS '所在楼层';
COMMENT ON COLUMN Bookshelf.Zone       IS '所在区域';

/************************************
* 7. Seat – 座位表
************************************/
CREATE TABLE Seat (
    SeatID     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, -- 主键，自增
    BuildingID INT NOT NULL,                                 -- 楼宇 ID
    Floor      INT NOT NULL,                                 -- 楼层
    SeatNumber VARCHAR2(20) NOT NULL,                        -- 座位号
    Zone       VARCHAR2(20),                                 -- 区域
    ReservationStatus VARCHAR2(10) NOT NULL,                 -- 状态：空闲/已预约
    CONSTRAINT uq_seat_loc UNIQUE (BuildingID, Floor, SeatNumber),
    CONSTRAINT fk_seat_building FOREIGN KEY (BuildingID)
        REFERENCES Building(BuildingID),
    CONSTRAINT chk_seat_status CHECK (ReservationStatus IN ('空闲','已预约'))
);
COMMENT ON TABLE Seat IS '自习座位表';
COMMENT ON COLUMN Seat.SeatID            IS '座位主键，自增 ID';
COMMENT ON COLUMN Seat.BuildingID        IS '所属楼宇 ID';
COMMENT ON COLUMN Seat.Floor             IS '所在楼层';
COMMENT ON COLUMN Seat.SeatNumber        IS '座位编号';
COMMENT ON COLUMN Seat.Zone              IS '区域';
COMMENT ON COLUMN Seat.ReservationStatus IS '预约状态：空闲 / 已预约';

/************************************
* 8. StudyRoom – 自习室表
************************************/
CREATE TABLE StudyRoom (
    RoomID     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, -- 自习室主键，自增 ID
    BuildingID INT NOT NULL,                                 -- 所属楼宇 ID
    Floor      INT NOT NULL,                                 -- 楼层
    RoomNumber VARCHAR2(20) NOT NULL,                        -- 房间号
    Zone       VARCHAR2(20),                                 -- 区域，如东区
    Capacity   INT NOT NULL,                                 -- 可容纳人数
    ReservationStatus VARCHAR2(10) NOT NULL,                 -- 状态：空闲/已预约
    CONSTRAINT uq_room_loc UNIQUE (BuildingID, Floor, RoomNumber),
    CONSTRAINT fk_studyroom_building FOREIGN KEY (BuildingID)
        REFERENCES Building(BuildingID),
    CONSTRAINT chk_room_status CHECK (ReservationStatus IN ('空闲','已预约'))
);
COMMENT ON TABLE StudyRoom IS '自习室信息表';
COMMENT ON COLUMN StudyRoom.RoomID            IS '自习室主键，自增 ID';
COMMENT ON COLUMN StudyRoom.BuildingID        IS '所属楼宇 ID';
COMMENT ON COLUMN StudyRoom.Floor             IS '所在楼层';
COMMENT ON COLUMN StudyRoom.RoomNumber        IS '房间号';
COMMENT ON COLUMN StudyRoom.Zone              IS '区域';
COMMENT ON COLUMN StudyRoom.Capacity          IS '容纳人数';
COMMENT ON COLUMN StudyRoom.ReservationStatus IS '预约状态：空闲 / 已预约';

/* ================================================================
   II. 核心业务表
   ================================================================ */

/************************************
* 9. Book – 馆藏实体书表
************************************/
/* 插入示例
    INSERT INTO Book (Status, ShelfID, ISBN)
    VALUES ('正常', 101, '9781594202667');
*/

CREATE TABLE Book (
    BookID  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, -- 实体书主键，自增 ID
    Barcode VARCHAR2(20) NOT NULL,                        -- 条码标签，业务唯一。一个ISBN可以对应多个Barcode，即一本书可以有多个副本。作用：借出时扫描书上这个条码，可以知道借出哪本。生成方式：触发器自动生成
    Status  VARCHAR2(10) NOT NULL,                        -- 状态：正常/下架/借出
    ShelfID INT,                                          -- 所在书架 ID
    ISBN    VARCHAR2(20) NOT NULL,                        -- 关联 ISBN
    CONSTRAINT uq_book_barcode UNIQUE (Barcode),
    CONSTRAINT fk_book_shelf  FOREIGN KEY (ShelfID) REFERENCES Bookshelf(ShelfID),
    CONSTRAINT fk_book_info   FOREIGN KEY (ISBN)    REFERENCES BookInfo(ISBN),
    CONSTRAINT chk_book_status CHECK (Status IN ('正常','下架','借出'))
);
COMMENT ON TABLE Book IS '馆藏实体书信息表';
COMMENT ON COLUMN Book.BookID  IS '实体书主键，自增 ID';
COMMENT ON COLUMN Book.Barcode IS '馆藏条码，唯一';
COMMENT ON COLUMN Book.Status  IS '图书状态：正常 / 下架 / 借出';
COMMENT ON COLUMN Book.ShelfID IS '所在书架 ID';
COMMENT ON COLUMN Book.ISBN    IS '对应图书 ISBN';


/************************************
* 10. Booklist – 书单表
************************************/
CREATE TABLE Booklist (
    BooklistID  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, -- 书单主键，自增 ID
    ListCode    VARCHAR2(20) NOT NULL,                        -- 书单业务编码，可分享
    BooklistName VARCHAR2(100) NOT NULL,                      -- 书单名称
    BooklistIntroduction CLOB,                                -- 书单简介
    CreatorID   INT,                                          -- 创建者 ReaderID
    CONSTRAINT uq_booklist_code UNIQUE (ListCode),
    CONSTRAINT fk_booklist_creator FOREIGN KEY (CreatorID)
        REFERENCES Reader(ReaderID)
);
COMMENT ON TABLE Booklist IS '读者创建的书单表';
COMMENT ON COLUMN Booklist.BooklistID           IS '书单主键，自增 ID';
COMMENT ON COLUMN Booklist.ListCode             IS '书单业务短码，唯一';
COMMENT ON COLUMN Booklist.BooklistName         IS '书单名称';
COMMENT ON COLUMN Booklist.BooklistIntroduction IS '书单简介/描述';
COMMENT ON COLUMN Booklist.CreatorID            IS '创建者 ReaderID';

/************************************
* 11. Comment_Table – 书评表
************************************/
CREATE TABLE Comment_Table (
    CommentID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, -- 评论主键，自增 ID
    ReaderID  INT NOT NULL,                                 -- 评论者 ReaderID
    ISBN      VARCHAR2(20) NOT NULL,                        -- 评论图书 ISBN
    Rating    INT,                                          -- 评分 1-5
    ReviewContent CLOB,                                     -- 评论内容
    CreateTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,         -- 创建时间
    Status VARCHAR2(10) DEFAULT '正常',                     -- 状态：正常/已删除
    CONSTRAINT fk_comment_reader  FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID),
    CONSTRAINT fk_comment_bookinfo FOREIGN KEY (ISBN)   REFERENCES BookInfo(ISBN),
    CONSTRAINT chk_comment_rating CHECK (Rating BETWEEN 1 AND 5),
    CONSTRAINT chk_comment_status CHECK (Status IN ('正常','已删除'))
);
COMMENT ON TABLE Comment_Table IS '图书评论表';
COMMENT ON COLUMN Comment_Table.CommentID     IS '评论主键，自增 ID';
COMMENT ON COLUMN Comment_Table.ReaderID      IS '评论者 ReaderID';
COMMENT ON COLUMN Comment_Table.ISBN          IS '被评论图书 ISBN';
COMMENT ON COLUMN Comment_Table.Rating        IS '评分 1‑5';
COMMENT ON COLUMN Comment_Table.ReviewContent IS '评论内容';
COMMENT ON COLUMN Comment_Table.CreateTime    IS '创建时间';
COMMENT ON COLUMN Comment_Table.Status        IS '评论状态：正常 / 已删除';


/************************************
* 12. Announcement – 公告表
************************************/
CREATE TABLE Announcement (
    AnnouncementID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, -- 公告主键
    LibrarianID INT NOT NULL,                                    -- 发布者管理员 ID
    Title   VARCHAR2(100) NOT NULL,                              -- 公告标题
    Content CLOB NOT NULL,                                       -- 公告内容
    CreateTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,              -- 发布时间
    TargetGroup VARCHAR2(10) NOT NULL,                           -- 目标群体
    Status      VARCHAR2(10) NOT NULL,                           -- 状态：发布中/已撤回
    CONSTRAINT fk_announcement_librarian FOREIGN KEY (LibrarianID)
        REFERENCES Librarian(LibrarianID),
    CONSTRAINT chk_announcement_target CHECK (TargetGroup IN ('所有人','读者','管理员')),
    CONSTRAINT chk_announcement_status CHECK (Status IN ('发布中','已撤回'))
);
COMMENT ON TABLE Announcement IS '系统公告表';
COMMENT ON COLUMN Announcement.AnnouncementID IS '公告主键，自增 ID';
COMMENT ON COLUMN Announcement.LibrarianID    IS '发布者 LibrarianID';
COMMENT ON COLUMN Announcement.Title         IS '公告标题';
COMMENT ON COLUMN Announcement.Content       IS '公告内容';
COMMENT ON COLUMN Announcement.CreateTime    IS '发布时间';
COMMENT ON COLUMN Announcement.TargetGroup   IS '目标群体';
COMMENT ON COLUMN Announcement.Status        IS '状态：发布中 / 已撤回';

/************************************
* 13. Log – 操作日志表
************************************/
CREATE TABLE Log (
    LogID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,    -- 日志主键
    OperationTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,     -- 操作时间
    OperationContent CLOB NOT NULL,                        -- 操作内容描述
    OperatorType VARCHAR2(10) NOT NULL,                    -- 操作者类型：Reader/Librarian
    OperatorID   INT,                                      -- 操作者 ID
    OperationStatus VARCHAR2(10) NOT NULL,                 -- 成功/失败
    ErrorMessage CLOB,                                     -- 错误信息
    CONSTRAINT chk_log_type   CHECK (OperatorType IN ('Reader','Librarian')),
    CONSTRAINT chk_log_status CHECK (OperationStatus IN ('成功','失败'))
);
COMMENT ON TABLE Log IS '系统操作日志表';
COMMENT ON COLUMN Log.LogID           IS '日志主键，自增 ID';
COMMENT ON COLUMN Log.OperationTime   IS '操作时间';
COMMENT ON COLUMN Log.OperationContent IS '操作内容';
COMMENT ON COLUMN Log.OperatorType    IS '操作者类型：Reader / Librarian';
COMMENT ON COLUMN Log.OperatorID      IS '操作者 ID';
COMMENT ON COLUMN Log.OperationStatus IS '操作结果：成功 / 失败';
COMMENT ON COLUMN Log.ErrorMessage    IS '错误信息';

/* ================================================================
   III. 记录 / 关联表
   ================================================================ */

/************************************
* 14. BorrowRecord – 借阅记录表
************************************/
CREATE TABLE BorrowRecord (
    BorrowRecordID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, -- 借阅记录主键
    ReaderID INT NOT NULL,                                       -- 借阅者 ID
    BookID   INT NOT NULL,                                       -- 实体书 ID
    BorrowTime TIMESTAMP NOT NULL,                               -- 借出时间
    ReturnTime TIMESTAMP,                                        -- 归还时间
    OverdueFine DECIMAL(5,2) DEFAULT 0,                          -- 逾期罚金
    CONSTRAINT fk_borrow_reader FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID),
    CONSTRAINT fk_borrow_book   FOREIGN KEY (BookID)   REFERENCES Book(BookID)
);
COMMENT ON TABLE BorrowRecord IS '图书借阅记录表';
COMMENT ON COLUMN BorrowRecord.BorrowRecordID IS '借阅记录主键，自增 ID';
COMMENT ON COLUMN BorrowRecord.ReaderID       IS '借阅者 ReaderID';
COMMENT ON COLUMN BorrowRecord.BookID         IS '借阅的 BookID';
COMMENT ON COLUMN BorrowRecord.BorrowTime     IS '借出时间';
COMMENT ON COLUMN BorrowRecord.ReturnTime     IS '归还时间';
COMMENT ON COLUMN BorrowRecord.OverdueFine    IS '逾期罚金';

/************************************
* 15. Reserve_Room – 自习室预约记录
************************************/
CREATE TABLE Reserve_Room (
    ReservationID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, -- 预约记录主键
    ReaderID INT NOT NULL,                                       -- 预约者 ID
    RoomID   INT NOT NULL,                                       -- 自习室 ID
    ReservationTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,         -- 预约提交时间
    StartTime TIMESTAMP NOT NULL,                                -- 使用开始
    EndTime   TIMESTAMP NOT NULL,                                -- 使用结束
    Status    VARCHAR2(10) NOT NULL,                             -- 已完成/未完成/取消
    CONSTRAINT fk_reserveroom_reader FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID),
    CONSTRAINT fk_reserveroom_room   FOREIGN KEY (RoomID)   REFERENCES StudyRoom(RoomID),
    CONSTRAINT chk_reserveroom_status CHECK (Status IN ('已完成','未完成','取消'))
);
COMMENT ON TABLE Reserve_Room IS '自习室预约记录表';
COMMENT ON COLUMN Reserve_Room.ReservationID   IS '预约记录主键，自增 ID';
COMMENT ON COLUMN Reserve_Room.ReaderID        IS '预约者 ReaderID';
COMMENT ON COLUMN Reserve_Room.RoomID          IS '预约的自习室 ID';
COMMENT ON COLUMN Reserve_Room.ReservationTime IS '预约提交时间';
COMMENT ON COLUMN Reserve_Room.StartTime       IS '使用开始时间';
COMMENT ON COLUMN Reserve_Room.EndTime         IS '使用结束时间';
COMMENT ON COLUMN Reserve_Room.Status          IS '状态：已完成 / 未完成 / 取消';

/************************************
* 16. Reserve_Seat – 座位预约记录表
************************************/
CREATE TABLE Reserve_Seat (
    ReservationID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, -- 预约记录主键
    ReaderID INT NOT NULL,                                       -- 预约者 ID
    SeatID   INT NOT NULL,                                       -- 座位 ID
    ReservationTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,         -- 预约提交时间
    StartTime TIMESTAMP NOT NULL,                                -- 使用开始
    EndTime   TIMESTAMP NOT NULL,                                -- 使用结束
    Status    VARCHAR2(10) NOT NULL,                             -- 已完成/未完成/取消
    CONSTRAINT fk_reserveseat_reader FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID),
    CONSTRAINT fk_reserveseat_seat   FOREIGN KEY (SeatID)   REFERENCES Seat(SeatID),
    CONSTRAINT chk_reserveseat_status CHECK (Status IN ('已完成','未完成','取消'))
);
COMMENT ON TABLE Reserve_Seat IS '座位预约记录表';
COMMENT ON COLUMN Reserve_Seat.ReservationID   IS '预约记录主键，自增 ID';
COMMENT ON COLUMN Reserve_Seat.ReaderID        IS '预约者 ReaderID';
COMMENT ON COLUMN Reserve_Seat.SeatID          IS '预约的 SeatID';
COMMENT ON COLUMN Reserve_Seat.ReservationTime IS '预约提交时间';
COMMENT ON COLUMN Reserve_Seat.StartTime       IS '使用开始时间';
COMMENT ON COLUMN Reserve_Seat.EndTime         IS '使用结束时间';
COMMENT ON COLUMN Reserve_Seat.Status          IS '状态：已完成 / 未完成 / 取消';

/************************************
* 17. Report – 举报记录表
************************************/
CREATE TABLE Report (
    ReportID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY, -- 举报记录主键
    CommentID INT NOT NULL,                               -- 被举报评论 ID
    ReaderID  INT NOT NULL,                               -- 举报者 ReaderID
    ReportReason CLOB,                                    -- 举报理由
    ReportTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,       -- 举报时间
    Status VARCHAR2(10) NOT NULL,                         -- 待处理/驳回/处理完成
    LibrarianID INT,                                      -- 处理管理员 ID
    CONSTRAINT fk_report_comment   FOREIGN KEY (CommentID)  REFERENCES Comment_Table(CommentID),
    CONSTRAINT fk_report_reader    FOREIGN KEY (ReaderID)   REFERENCES Reader(ReaderID),
    CONSTRAINT fk_report_librarian FOREIGN KEY (LibrarianID) REFERENCES Librarian(LibrarianID),
    CONSTRAINT chk_report_status   CHECK (Status IN ('待处理','驳回','处理完成'))
);
COMMENT ON TABLE Report IS '评论举报记录表';
COMMENT ON COLUMN Report.ReportID     IS '举报记录主键，自增 ID';
COMMENT ON COLUMN Report.CommentID    IS '被举报评论 ID';
COMMENT ON COLUMN Report.ReaderID     IS '举报者 ReaderID';
COMMENT ON COLUMN Report.ReportReason IS '举报理由';
COMMENT ON COLUMN Report.ReportTime   IS '举报时间';
COMMENT ON COLUMN Report.Status       IS '处理状态：待处理 / 驳回 / 处理完成';
COMMENT ON COLUMN Report.LibrarianID  IS '处理该举报的管理员 ID';

/************************************
* 18. Booklist_Book – 书单-图书关联表
************************************/
CREATE TABLE Booklist_Book (
    BooklistID INT,                                         -- 书单 ID
    ISBN       VARCHAR2(20),                                -- 图书 ISBN
    AddTime    TIMESTAMP,                                   -- 加入书单时间
    Notes      CLOB,                                        -- 备注
    CONSTRAINT pk_booklist_book PRIMARY KEY (BooklistID, ISBN),
    CONSTRAINT fk_bb_booklist FOREIGN KEY (BooklistID) REFERENCES Booklist(BooklistID),
    CONSTRAINT fk_bb_bookinfo FOREIGN KEY (ISBN)       REFERENCES BookInfo(ISBN)
);
COMMENT ON TABLE Booklist_Book IS '书单与图书多对多关联';
COMMENT ON COLUMN Booklist_Book.BooklistID IS '书单 ID';
COMMENT ON COLUMN Booklist_Book.ISBN       IS '图书 ISBN';
COMMENT ON COLUMN Booklist_Book.AddTime    IS '加入书单的时间';
COMMENT ON COLUMN Booklist_Book.Notes      IS '备注';

/************************************
* 19. Book_Classify – 图书-分类关联表
************************************/
CREATE TABLE Book_Classify (
    ISBN       VARCHAR2(20),                                -- 图书 ISBN
    CategoryID INT,                                        -- 分类 ID
    RelationNote CLOB,                                     -- 备注
    CONSTRAINT pk_book_classify PRIMARY KEY (ISBN, CategoryID),
    CONSTRAINT fk_bc_bookinfo FOREIGN KEY (ISBN)       REFERENCES BookInfo(ISBN),
    CONSTRAINT fk_bc_category FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID)
);
COMMENT ON TABLE Book_Classify IS '图书与分类关联表';
COMMENT ON COLUMN Book_Classify.ISBN       IS '图书 ISBN';
COMMENT ON COLUMN Book_Classify.CategoryID IS '分类 ID';
COMMENT ON COLUMN Book_Classify.RelationNote IS '关联备注';

/************************************
* 20. Collect – 书单收藏表
************************************/
CREATE TABLE Collect (
    BooklistID INT,                                         -- 书单 ID
    ReaderID   INT,                                         -- 收藏者 ID
    FavoriteTime TIMESTAMP,                                 -- 收藏时间
    Notes      CLOB,                                        -- 备注
    CONSTRAINT pk_collect PRIMARY KEY (BooklistID, ReaderID),
    CONSTRAINT fk_collect_booklist FOREIGN KEY (BooklistID) REFERENCES Booklist(BooklistID),
    CONSTRAINT fk_collect_reader   FOREIGN KEY (ReaderID)   REFERENCES Reader(ReaderID)
);
COMMENT ON TABLE Collect IS '书单收藏记录表';
COMMENT ON COLUMN Collect.BooklistID   IS '书单 ID';
COMMENT ON COLUMN Collect.ReaderID     IS '收藏者 ReaderID';
COMMENT ON COLUMN Collect.FavoriteTime IS '收藏时间';
COMMENT ON COLUMN Collect.Notes        IS '备注';

// 21
CREATE TABLE PurchaseLog (
    LogID NUMBER GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    LogText NVARCHAR2(1000) NOT NULL,
    AdminID NVARCHAR2(50), -- 此字段为未来扩展保留，可用于记录操作员
    LogDate DATE DEFAULT SYSDATE NOT NULL
);

COMMENT ON TABLE PurchaseLog IS '用于存储采购分析页面的管理员操作日志和备注';
COMMENT ON COLUMN PurchaseLog.LogID IS '日志主键';
COMMENT ON COLUMN PurchaseLog.LogText IS '管理员填写的日志内容';
COMMENT ON COLUMN PurchaseLog.AdminID IS '进行操作的管理员ID';
COMMENT ON COLUMN PurchaseLog.LogDate IS '日志创建时间';