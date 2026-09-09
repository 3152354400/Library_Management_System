-- ================================================================
-- 图书遗失/损坏赔偿登记表
-- 存储读者遗失或损坏图书后的赔偿处理记录
-- ================================================================
CREATE TABLE BookLossReport (
    ReportID INT PRIMARY KEY,
    ReaderID INT NOT NULL,
    BookID VARCHAR2(20) NOT NULL,
    ReportType VARCHAR2(10) NOT NULL CHECK (ReportType IN ('遗失', '损坏')),
    ReportTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Description VARCHAR2(500),
    EstimatedValue DECIMAL(10,2),
    CompensationAmount DECIMAL(10,2),
    Status VARCHAR2(20) DEFAULT '待处理' CHECK (Status IN ('待处理', '已确认', '已赔偿', '已取消')),
    HandleTime TIMESTAMP,
    HandlerID INT,
    HandleResult VARCHAR2(500),
    PaymentStatus VARCHAR2(10) DEFAULT '未缴纳' CHECK (PaymentStatus IN ('未缴纳', '已缴纳', '已减免')),
    PaymentTime TIMESTAMP,
    Remark VARCHAR2(500),
    CONSTRAINT fk_loss_reader FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID),
    CONSTRAINT fk_loss_book FOREIGN KEY (BookID) REFERENCES Book(BookID),
    CONSTRAINT fk_loss_handler FOREIGN KEY (HandlerID) REFERENCES Librarian(LibrarianID)
);

COMMENT ON TABLE BookLossReport IS '图书遗失/损坏赔偿登记表：记录读者遗失或损坏图书的赔偿处理';
COMMENT ON COLUMN BookLossReport.ReportID IS '赔偿报告编号，主键';
COMMENT ON COLUMN BookLossReport.ReaderID IS '读者编号，外键';
COMMENT ON COLUMN BookLossReport.BookID IS '图书ID，外键';
COMMENT ON COLUMN BookLossReport.ReportType IS '报告类型：遗失、损坏';
COMMENT ON COLUMN BookLossReport.ReportTime IS '报告时间';
COMMENT ON COLUMN BookLossReport.Description IS '详细描述';
COMMENT ON COLUMN BookLossReport.EstimatedValue IS '图书估值';
COMMENT ON COLUMN BookLossReport.CompensationAmount IS '实际赔偿金额';
COMMENT ON COLUMN BookLossReport.Status IS '处理状态：待处理、已确认、已赔偿、已取消';
COMMENT ON COLUMN BookLossReport.HandleTime IS '处理时间';
COMMENT ON COLUMN BookLossReport.HandlerID IS '处理管理员';
COMMENT ON COLUMN BookLossReport.HandleResult IS '处理结果说明';
COMMENT ON COLUMN BookLossReport.PaymentStatus IS '缴纳状态：未缴纳、已缴纳、已减免';
COMMENT ON COLUMN BookLossReport.PaymentTime IS '实际缴纳时间';
COMMENT ON COLUMN BookLossReport.Remark IS '备注';

-- 赔偿报告ID序列
CREATE SEQUENCE seq_report_id START WITH 1 INCREMENT BY 1;

-- 赔偿报告索引
CREATE INDEX idx_loss_reader ON BookLossReport(ReaderID);
CREATE INDEX idx_loss_book ON BookLossReport(BookID);
CREATE INDEX idx_loss_status ON BookLossReport(Status);
