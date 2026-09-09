-- ================================================================
-- 扩展功能的索引优化
-- ================================================================

-- RenewRecord表索引
CREATE INDEX idx_renew_borrow ON RenewRecord(BorrowRecordID);
CREATE INDEX idx_renew_reader ON RenewRecord(ReaderID);
CREATE INDEX idx_renew_time ON RenewRecord(RenewTime DESC);

-- Fine表索引
CREATE INDEX idx_fine_reader ON Fine(ReaderID);
CREATE INDEX idx_fine_status ON Fine(PayStatus);
CREATE INDEX idx_fine_date ON Fine(FineDate DESC);
CREATE INDEX idx_fine_unpaid ON Fine(ReaderID, PayStatus) WHERE PayStatus IN ('未缴纳', '部分缴纳');

-- FinePayment表索引
CREATE INDEX idx_payment_fine ON FinePayment(FineID);
CREATE INDEX idx_payment_time ON FinePayment(PayTime DESC);

-- ReserveBook表索引
CREATE INDEX idx_reserve_isbn_status ON ReserveBook(ISBN, Status);
CREATE INDEX idx_reserve_reader_status ON ReserveBook(ReaderID, Status);
CREATE INDEX idx_reserve_time ON ReserveBook(ReserveTime ASC);
CREATE INDEX idx_reserve_expire ON ReserveBook(ExpireTime);

-- Notification表索引
CREATE INDEX idx_notif_reader_read ON Notification(ReaderID, IsRead);
CREATE INDEX idx_notif_type ON Notification(Type);
CREATE INDEX idx_notif_time ON Notification(CreateTime DESC);

-- RecommendPurchase表索引
CREATE INDEX idx_recommend_reader ON RecommendPurchase(ReaderID);
CREATE INDEX idx_recommend_status ON RecommendPurchase(Status);
CREATE INDEX idx_recommend_time ON RecommendPurchase(RecommendTime DESC);

-- OperationLog表索引
CREATE INDEX idx_log_time ON OperationLog(OperationTime DESC);
CREATE INDEX idx_log_operator ON OperationLog(OperatorID, OperatorType);
CREATE INDEX idx_log_module ON OperationLog(OperationModule);
CREATE INDEX idx_log_status ON OperationLog(Status);

-- 扩展BorrowRecord的索引
CREATE INDEX idx_borrow_duetime_status ON BorrowRecord(DueTime, Status) WHERE Status IN ('借阅中', '借出');

-- 扩展Reader表的索引
CREATE INDEX idx_reader_credit ON Reader(CreditScore);
