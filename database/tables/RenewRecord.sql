-- ================================================================
-- 续借记录表
-- 存储读者续借图书的历史记录
-- ================================================================
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

COMMENT ON TABLE RenewRecord IS '续借记录表：存储读者续借图书的历史信息';
COMMENT ON COLUMN RenewRecord.RenewID IS '续借编号，主键';
COMMENT ON COLUMN RenewRecord.BorrowRecordID IS '原借阅记录编号，外键';
COMMENT ON COLUMN RenewRecord.ReaderID IS '读者编号，外键';
COMMENT ON COLUMN RenewRecord.RenewTime IS '续借时间';
COMMENT ON COLUMN RenewRecord.OldDueTime IS '原应还时间';
COMMENT ON COLUMN RenewRecord.NewDueTime IS '新应还时间';
COMMENT ON COLUMN RenewRecord.RenewCount IS '续借次数';
COMMENT ON COLUMN RenewRecord.Status IS '续借状态：有效、已还、已逾期';
COMMENT ON COLUMN RenewRecord.Reason IS '续借原因';
COMMENT ON COLUMN RenewRecord.OperatorID IS '操作管理员';
COMMENT ON COLUMN RenewRecord.Remark IS '备注';

-- 续借ID序列
CREATE SEQUENCE seq_renew_id START WITH 1 INCREMENT BY 1;

-- 续借记录索引
CREATE INDEX idx_renew_borrow ON RenewRecord(BorrowRecordID);
CREATE INDEX idx_renew_reader ON RenewRecord(ReaderID);
CREATE INDEX idx_renew_time ON RenewRecord(RenewTime DESC);
