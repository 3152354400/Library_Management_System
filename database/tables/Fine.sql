-- ================================================================
-- 罚款明细表
-- 存储读者的各类罚款记录
-- ================================================================
CREATE TABLE Fine (
    FineID INT PRIMARY KEY,
    ReaderID INT NOT NULL,
    BorrowRecordID INT,
    FineType VARCHAR2(20) NOT NULL CHECK (FineType IN ('逾期罚款', '损坏赔偿', '丢失赔偿', '违规罚款')),
    Amount DECIMAL(8,2) NOT NULL CHECK (Amount >= 0),
    PaidAmount DECIMAL(8,2) DEFAULT 0 CHECK (PaidAmount >= 0),
    FineDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    DueDate TIMESTAMP,
    PayStatus VARCHAR2(10) DEFAULT '未缴纳' CHECK (PayStatus IN ('未缴纳', '部分缴纳', '已缴纳', '已减免')),
    PayTime TIMESTAMP,
    HandlerID INT,
    Remark VARCHAR2(500),
    CONSTRAINT fk_fine_reader FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID),
    CONSTRAINT fk_fine_borrow FOREIGN KEY (BorrowRecordID) REFERENCES BorrowRecord(BorrowRecordID),
    CONSTRAINT fk_fine_handler FOREIGN KEY (HandlerID) REFERENCES Librarian(LibrarianID),
    CONSTRAINT chk_fine_amount CHECK (Amount >= PaidAmount)
);

COMMENT ON TABLE Fine IS '罚款明细表：存储读者的逾期罚款、损坏赔偿等各类罚款';
COMMENT ON COLUMN Fine.FineID IS '罚款编号，主键';
COMMENT ON COLUMN Fine.ReaderID IS '读者编号，外键';
COMMENT ON COLUMN Fine.BorrowRecordID IS '关联借阅记录编号，外键';
COMMENT ON COLUMN Fine.FineType IS '罚款类型：逾期罚款、损坏赔偿、丢失赔偿、违规罚款';
COMMENT ON COLUMN Fine.Amount IS '罚款金额';
COMMENT ON COLUMN Fine.PaidAmount IS '已缴纳金额';
COMMENT ON COLUMN Fine.FineDate IS '罚款生成时间';
COMMENT ON COLUMN Fine.DueDate IS '缴纳截止日期';
COMMENT ON COLUMN Fine.PayStatus IS '缴纳状态：未缴纳、部分缴纳、已缴纳、已减免';
COMMENT ON COLUMN Fine.PayTime IS '实际缴纳时间';
COMMENT ON COLUMN Fine.HandlerID IS '处理该罚款的管理员';
COMMENT ON COLUMN Fine.Remark IS '备注说明';

-- 罚款ID序列
CREATE SEQUENCE seq_fine_id START WITH 1 INCREMENT BY 1;

-- 罚款记录索引
CREATE INDEX idx_fine_reader ON Fine(ReaderID);
CREATE INDEX idx_fine_status ON Fine(PayStatus);
CREATE INDEX idx_fine_date ON Fine(FineDate DESC);

-- ================================================================
-- 罚款缴纳记录表
-- 存储罚款的多次缴纳明细
-- ================================================================
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

COMMENT ON TABLE FinePayment IS '罚款缴纳记录表：存储罚款的多次缴纳明细';
COMMENT ON COLUMN FinePayment.PaymentID IS '缴纳记录编号，主键';
COMMENT ON COLUMN FinePayment.FineID IS '所属罚款编号，外键';
COMMENT ON COLUMN FinePayment.PayAmount IS '本次缴纳金额';
COMMENT ON COLUMN FinePayment.PayTime IS '缴纳时间';
COMMENT ON COLUMN FinePayment.PayMethod IS '缴纳方式：现金、微信、支付宝、银行卡、其他';
COMMENT ON COLUMN FinePayment.OperatorID IS '操作管理员';
COMMENT ON COLUMN FinePayment.Remark IS '备注';

CREATE SEQUENCE seq_payment_id START WITH 1 INCREMENT BY 1;
CREATE INDEX idx_payment_fine ON FinePayment(FineID);
