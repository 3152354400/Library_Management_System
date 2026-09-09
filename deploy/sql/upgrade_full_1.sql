SET SQLBLANKLINES ON
SET DEFINE OFF
-- ================================================================
-- 完整版升级 SQL（第一部分）：建表 + 加列 + 序列
-- 目标：在现有 15 表基础上补齐 dev 完整版全部数据库对象
-- 执行前请确认：已用 SET SQLBLANKLINES ON
-- ================================================================

-- ---------- 1. 扩展表 ----------

-- 座位表
CREATE TABLE Seat (
    SeatID     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    BuildingID INT NOT NULL,
    Floor      INT NOT NULL,
    SeatNumber VARCHAR2(20) NOT NULL,
    Zone       VARCHAR2(20),
    ReservationStatus VARCHAR2(10) NOT NULL,
    CONSTRAINT uq_seat_loc UNIQUE (BuildingID, Floor, SeatNumber),
    CONSTRAINT fk_seat_building FOREIGN KEY (BuildingID) REFERENCES Building(BuildingID),
    CONSTRAINT chk_seat_status CHECK (ReservationStatus IN ('空闲','已预约'))
);

-- 自习室表
CREATE TABLE StudyRoom (
    RoomID     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    BuildingID INT NOT NULL,
    Floor      INT NOT NULL,
    RoomNumber VARCHAR2(20) NOT NULL,
    Zone       VARCHAR2(20),
    Capacity   INT NOT NULL,
    ReservationStatus VARCHAR2(10) NOT NULL,
    CONSTRAINT uq_room_loc UNIQUE (BuildingID, Floor, RoomNumber),
    CONSTRAINT fk_studyroom_building FOREIGN KEY (BuildingID) REFERENCES Building(BuildingID),
    CONSTRAINT chk_room_status CHECK (ReservationStatus IN ('空闲','已预约'))
);

-- 自习室预约记录表
CREATE TABLE Reserve_Room (
    ReservationID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ReaderID INT NOT NULL,
    RoomID   INT NOT NULL,
    ReservationTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    StartTime TIMESTAMP NOT NULL,
    EndTime   TIMESTAMP NOT NULL,
    Status    VARCHAR2(10) NOT NULL,
    CONSTRAINT fk_reserveroom_reader FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID),
    CONSTRAINT fk_reserveroom_room   FOREIGN KEY (RoomID)   REFERENCES StudyRoom(RoomID),
    CONSTRAINT chk_reserveroom_status CHECK (Status IN ('已完成','未完成','取消'))
);

-- 座位预约记录表
CREATE TABLE Reserve_Seat (
    ReservationID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ReaderID INT NOT NULL,
    SeatID   INT NOT NULL,
    ReservationTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    StartTime TIMESTAMP NOT NULL,
    EndTime   TIMESTAMP NOT NULL,
    Status    VARCHAR2(10) NOT NULL,
    CONSTRAINT fk_reserveseat_reader FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID),
    CONSTRAINT fk_reserveseat_seat   FOREIGN KEY (SeatID)   REFERENCES Seat(SeatID),
    CONSTRAINT chk_reserveseat_status CHECK (Status IN ('已完成','未完成','取消'))
);

-- 续借记录表
CREATE TABLE RenewRecord (
    RenewID INT PRIMARY KEY,
    BorrowRecordID INT NOT NULL,
    ReaderID INT NOT NULL,
    RenewTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    OldDueTime TIMESTAMP NOT NULL,
    NewDueTime TIMESTAMP NOT NULL,
    RenewCount INT DEFAULT 1,
    Reason VARCHAR2(200),
    Status VARCHAR2(10) DEFAULT '有效' CHECK (Status IN ('有效', '已还', '已逾期')),
    OperatorID INT,
    Remark VARCHAR2(500),
    CONSTRAINT fk_renew_borrow FOREIGN KEY (BorrowRecordID) REFERENCES BorrowRecord(BorrowRecordID),
    CONSTRAINT fk_renew_reader FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID),
    CONSTRAINT fk_renew_operator FOREIGN KEY (OperatorID) REFERENCES Librarian(LibrarianID)
);

-- 罚款缴纳记录表
CREATE TABLE FinePayment (
    PaymentID INT PRIMARY KEY,
    FineID INT NOT NULL,
    PayAmount DECIMAL(8,2) NOT NULL CHECK (PayAmount > 0),
    PayTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PayMethod VARCHAR2(20) CHECK (PayMethod IN ('现金', '微信', '支付宝', '银行卡', '其他')),
    OperatorID INT NOT NULL,
    Remark VARCHAR2(200),
    CONSTRAINT fk_payment_fine FOREIGN KEY (FineID) REFERENCES Fine(FineID),
    CONSTRAINT fk_payment_operator FOREIGN KEY (OperatorID) REFERENCES Librarian(LibrarianID)
);

-- 操作日志表
CREATE TABLE OperationLog (
    LogID INT PRIMARY KEY,
    OperationTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    OperatorID VARCHAR2(20) NOT NULL,
    OperatorType VARCHAR2(10) NOT NULL CHECK (OperatorType IN ('Reader', 'Librarian', '系统')),
    OperationModule VARCHAR2(50) NOT NULL,
    OperationAction VARCHAR2(50) NOT NULL,
    TargetType VARCHAR2(30),
    TargetID VARCHAR2(50),
    OperationDetail VARCHAR2(1000),
    BeforeValue VARCHAR2(500),
    AfterValue VARCHAR2(500),
    IPAddress VARCHAR2(45),
    UserAgent VARCHAR2(500),
    Status VARCHAR2(10) DEFAULT '成功' CHECK (Status IN ('成功', '失败', '异常')),
    ErrorMessage VARCHAR2(1000),
    Duration INT,
    CONSTRAINT fk_log_operator_reader FOREIGN KEY (OperatorID) REFERENCES Reader(ReaderID),
    CONSTRAINT fk_log_operator_librarian FOREIGN KEY (OperatorID) REFERENCES Librarian(LibrarianID)
);

-- 图书预约表
CREATE TABLE ReserveBook (
    ReserveID INT PRIMARY KEY,
    ReaderID INT NOT NULL,
    ISBN VARCHAR2(20) NOT NULL,
    ReserveTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ExpectedBorrowTime TIMESTAMP NOT NULL,
    ExpireTime TIMESTAMP NOT NULL,
    NotifyTime TIMESTAMP,
    Status VARCHAR2(10) DEFAULT '等待中' CHECK (Status IN ('等待中', '已通知', '已借出', '已取消', '已过期')),
    Priority INT DEFAULT 0,
    CancelReason VARCHAR2(200),
    HandleTime TIMESTAMP,
    HandlerID INT,
    Remark VARCHAR2(500),
    CONSTRAINT fk_reserve_reader FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID),
    CONSTRAINT fk_reserve_bookinfo FOREIGN KEY (ISBN) REFERENCES BookInfo(ISBN),
    CONSTRAINT fk_reserve_handler FOREIGN KEY (HandlerID) REFERENCES Librarian(LibrarianID)
);

-- 图书荐购表
CREATE TABLE RecommendPurchase (
    RecommendID INT PRIMARY KEY,
    ReaderID INT NOT NULL,
    ISBN VARCHAR2(20),
    Title VARCHAR2(200) NOT NULL,
    Author VARCHAR2(100),
    Publisher VARCHAR2(100),
    PublishYear INT,
    Reason VARCHAR2(500),
    RecommendTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR2(20) DEFAULT '待审核' CHECK (Status IN ('待审核', '已采纳', '被拒绝', '已购买')),
    HandlerID INT,
    HandleTime TIMESTAMP,
    HandleResult VARCHAR2(500),
    PurchasePrice DECIMAL(8,2),
    PurchaseTime TIMESTAMP,
    Remark VARCHAR2(500),
    CONSTRAINT fk_recommend_reader FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID),
    CONSTRAINT fk_recommend_handler FOREIGN KEY (HandlerID) REFERENCES Librarian(LibrarianID)
);

-- 消息通知表
CREATE TABLE Notification (
    NotificationID INT PRIMARY KEY,
    ReaderID INT NOT NULL,
    Title VARCHAR2(100) NOT NULL,
    Content VARCHAR2(1000) NOT NULL,
    Type VARCHAR2(20) NOT NULL CHECK (Type IN ('逾期提醒', '预约到书', '罚款通知', '系统公告', '评论回复', '借阅成功', '还书提醒', '其他')),
    Priority VARCHAR2(10) DEFAULT '普通' CHECK (Priority IN ('普通', '重要', '紧急')),
    CreateTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    IsRead CHAR(1) DEFAULT 'N' CHECK (IsRead IN ('Y', 'N')),
    ReadTime TIMESTAMP,
    RelatedType VARCHAR2(20),
    RelatedID INT,
    SenderID INT,
    SenderType VARCHAR2(10) CHECK (SenderType IN ('系统', '管理员', '读者')),
    ExpireTime TIMESTAMP,
    Status VARCHAR2(10) DEFAULT '有效' CHECK (Status IN ('有效', '已删除', '已过期')),
    CONSTRAINT fk_notif_reader FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID),
    CONSTRAINT fk_notif_sender FOREIGN KEY (SenderID) REFERENCES Librarian(LibrarianID)
);

-- 采购日志表（采购分析页面）
CREATE TABLE PurchaseLog (
    LogID NUMBER GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    LogText NVARCHAR2(1000) NOT NULL,
    AdminID NVARCHAR2(50),
    LogDate DATE DEFAULT SYSDATE NOT NULL
);

-- ---------- 2. 现有表加列 ----------

-- Fine 表补完整版列（罚款类型/截止日/缴纳时间）
ALTER TABLE Fine ADD (FineType VARCHAR2(20), DueDate TIMESTAMP, PayTime TIMESTAMP);

-- Book_Classify 补 RelationNote（dev 完整版有该列）
ALTER TABLE Book_Classify ADD (RelationNote CLOB);

-- ---------- 3. 序列 ----------

CREATE SEQUENCE seq_barcode START WITH 1 INCREMENT BY 1 MINVALUE 1 MAXVALUE 99999 CYCLE CACHE 20;
CREATE SEQUENCE seq_renew_id START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_fine_id START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_payment_id START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_reserve_id START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_recommend_id START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_notif_id START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_log_id START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_report_id START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_favorite_id START WITH 1 INCREMENT BY 1;

-- ---------- 4. 示例数据：座位与自习室（基于现有 Building） ----------

INSERT INTO Seat (BuildingID, Floor, SeatNumber, Zone, ReservationStatus)
SELECT BuildingID, 1, 'A0' || LPAD(LEVEL, 2, '0'), '东区', '空闲'
FROM Building
WHERE ROWNUM = 1
CONNECT BY LEVEL <= 12;

INSERT INTO StudyRoom (BuildingID, Floor, RoomNumber, Zone, Capacity, ReservationStatus)
SELECT BuildingID, 1, '101', '东区', 20, '空闲' FROM Building WHERE ROWNUM = 1;
INSERT INTO StudyRoom (BuildingID, Floor, RoomNumber, Zone, Capacity, ReservationStatus)
SELECT BuildingID, 1, '102', '东区', 30, '空闲' FROM Building WHERE ROWNUM = 1;
INSERT INTO StudyRoom (BuildingID, Floor, RoomNumber, Zone, Capacity, ReservationStatus)
SELECT BuildingID, 2, '201', '西区', 40, '空闲' FROM Building WHERE ROWNUM = 1;

COMMIT;
