-- ================================================================
-- 消息通知表
-- 存储系统向读者发送的各种通知消息
-- ================================================================
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

COMMENT ON TABLE Notification IS '消息通知表：存储系统向读者发送的各种通知消息';
COMMENT ON COLUMN Notification.NotificationID IS '通知编号，主键';
COMMENT ON COLUMN Notification.ReaderID IS '接收通知的读者编号，外键';
COMMENT ON COLUMN Notification.Title IS '通知标题';
COMMENT ON COLUMN Notification.Content IS '通知内容';
COMMENT ON COLUMN Notification.Type IS '通知类型：逾期提醒、预约到书、罚款通知、系统公告等';
COMMENT ON COLUMN Notification.Priority IS '优先级：普通、重要、紧急';
COMMENT ON COLUMN Notification.CreateTime IS '通知发送时间';
COMMENT ON COLUMN Notification.IsRead IS '是否已读：Y/N';
COMMENT ON COLUMN Notification.ReadTime IS '阅读时间';
COMMENT ON COLUMN Notification.RelatedType IS '关联业务类型';
COMMENT ON COLUMN Notification.RelatedID IS '关联业务ID';
COMMENT ON COLUMN Notification.SenderID IS '发送者ID';
COMMENT ON COLUMN Notification.SenderType IS '发送者类型：系统、管理员、读者';
COMMENT ON COLUMN Notification.ExpireTime IS '通知过期时间';
COMMENT ON COLUMN Notification.Status IS '状态：有效、已删除、已过期';

CREATE SEQUENCE seq_notif_id START WITH 1 INCREMENT BY 1;

-- 通知索引
CREATE INDEX idx_notif_reader_read ON Notification(ReaderID, IsRead);
CREATE INDEX idx_notif_type ON Notification(Type);
CREATE INDEX idx_notif_time ON Notification(CreateTime DESC);
CREATE INDEX idx_notif_status ON Notification(Status);

-- ================================================================
-- 操作日志表
-- 记录所有管理员和读者的关键操作行为
-- ================================================================
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

COMMENT ON TABLE OperationLog IS '操作日志表：记录所有关键操作行为，用于审计追踪';
COMMENT ON COLUMN OperationLog.LogID IS '日志编号，主键';
COMMENT ON COLUMN OperationLog.OperationTime IS '操作时间';
COMMENT ON COLUMN OperationLog.OperatorID IS '操作者ID';
COMMENT ON COLUMN OperationLog.OperatorType IS '操作者类型：Reader、Librarian、系统';
COMMENT ON COLUMN OperationLog.OperationModule IS '操作模块：借阅、还书、图书管理等';
COMMENT ON COLUMN OperationLog.OperationAction IS '操作动作：借书、还书、修改等';
COMMENT ON COLUMN OperationLog.TargetType IS '操作对象类型';
COMMENT ON COLUMN OperationLog.TargetID IS '操作对象ID';
COMMENT ON COLUMN OperationLog.OperationDetail IS '操作详情描述';
COMMENT ON COLUMN OperationLog.BeforeValue IS '操作前值';
COMMENT ON COLUMN OperationLog.AfterValue IS '操作后值';
COMMENT ON COLUMN OperationLog.IPAddress IS '操作者IP地址';
COMMENT ON COLUMN OperationLog.UserAgent IS '浏览器/客户端信息';
COMMENT ON COLUMN OperationLog.Status IS '操作状态：成功、失败、异常';
COMMENT ON COLUMN OperationLog.ErrorMessage IS '错误信息';
COMMENT ON COLUMN OperationLog.Duration IS '操作耗时（毫秒）';

CREATE SEQUENCE seq_log_id START WITH 1 INCREMENT BY 1;

-- 日志索引
CREATE INDEX idx_log_time ON OperationLog(OperationTime DESC);
CREATE INDEX idx_log_operator ON OperationLog(OperatorID, OperatorType);
CREATE INDEX idx_log_module ON OperationLog(OperationModule);
CREATE INDEX idx_log_status ON OperationLog(Status);
