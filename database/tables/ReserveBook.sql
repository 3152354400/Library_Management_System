-- ================================================================
-- 图书预约表
-- 存储读者预约图书的记录，当图书全部借出时可预约排队
-- ================================================================
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

COMMENT ON TABLE ReserveBook IS '图书预约表：存储读者预约图书的记录，支持排队等待';
COMMENT ON COLUMN ReserveBook.ReserveID IS '预约编号，主键';
COMMENT ON COLUMN ReserveBook.ReaderID IS '预约读者编号，外键';
COMMENT ON COLUMN ReserveBook.ISBN IS '预约图书ISBN，外键';
COMMENT ON COLUMN ReserveBook.ReserveTime IS '预约时间';
COMMENT ON COLUMN ReserveBook.ExpectedBorrowTime IS '期望借书时间';
COMMENT ON COLUMN ReserveBook.ExpireTime IS '预约过期时间（预约成功后保留3天）';
COMMENT ON COLUMN ReserveBook.NotifyTime IS '通知读者取书时间';
COMMENT ON COLUMN ReserveBook.Status IS '预约状态：等待中、已通知、已借出、已取消、已过期';
COMMENT ON COLUMN ReserveBook.Priority IS '优先级（数字越大优先级越高）';
COMMENT ON COLUMN ReserveBook.CancelReason IS '取消原因';
COMMENT ON COLUMN ReserveBook.HandleTime IS '处理时间';
COMMENT ON COLUMN ReserveBook.HandlerID IS '处理管理员';
COMMENT ON COLUMN ReserveBook.Remark IS '备注';

CREATE SEQUENCE seq_reserve_id START WITH 1 INCREMENT BY 1;

-- 预约记录索引
CREATE INDEX idx_reserve_isbn_status ON ReserveBook(ISBN, Status);
CREATE INDEX idx_reserve_reader ON ReserveBook(ReaderID);
CREATE INDEX idx_reserve_time ON ReserveBook(ReserveTime ASC);
CREATE INDEX idx_reserve_expire ON ReserveBook(ExpireTime);

-- ================================================================
-- 图书荐购表
-- 存储读者推荐图书馆采购的图书
-- ================================================================
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

COMMENT ON TABLE RecommendPurchase IS '图书荐购表：存储读者推荐图书馆采购的图书信息';
COMMENT ON COLUMN RecommendPurchase.RecommendID IS '荐购编号，主键';
COMMENT ON COLUMN RecommendPurchase.ReaderID IS '推荐读者编号，外键';
COMMENT ON COLUMN RecommendPurchase.ISBN IS 'ISBN（可为空表示新书推荐）';
COMMENT ON COLUMN RecommendPurchase.Title IS '书名，非空';
COMMENT ON COLUMN RecommendPurchase.Author IS '作者';
COMMENT ON COLUMN RecommendPurchase.Publisher IS '出版社';
COMMENT ON COLUMN RecommendPurchase.PublishYear IS '出版年份';
COMMENT ON COLUMN RecommendPurchase.Reason IS '推荐理由';
COMMENT ON COLUMN RecommendPurchase.RecommendTime IS '推荐时间';
COMMENT ON COLUMN RecommendPurchase.Status IS '处理状态：待审核、已采纳、被拒绝、已购买';
COMMENT ON COLUMN RecommendPurchase.HandlerID IS '处理管理员';
COMMENT ON COLUMN RecommendPurchase.HandleTime IS '处理时间';
COMMENT ON COLUMN RecommendPurchase.HandleResult IS '处理结果/回复';
COMMENT ON COLUMN RecommendPurchase.PurchasePrice IS '采购价格';
COMMENT ON COLUMN RecommendPurchase.PurchaseTime IS '实际采购时间';
COMMENT ON COLUMN RecommendPurchase.Remark IS '备注';

CREATE SEQUENCE seq_recommend_id START WITH 1 INCREMENT BY 1;
CREATE INDEX idx_recommend_reader ON RecommendPurchase(ReaderID);
CREATE INDEX idx_recommend_status ON RecommendPurchase(Status);
CREATE INDEX idx_recommend_time ON RecommendPurchase(RecommendTime DESC);
