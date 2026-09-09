-- ================================================================
-- 还书时自动创建罚款触发器
-- 当借阅记录由借阅中变为已归还，且产生罚款时自动生成罚款记录
-- ================================================================
CREATE OR REPLACE TRIGGER trg_create_fine_on_return
AFTER UPDATE ON BorrowRecord
FOR EACH ROW
WHEN (OLD.ReturnTime IS NULL 
      AND NEW.ReturnTime IS NOT NULL 
      AND NEW.OverdueFine > 0)
DECLARE
    v_fine_id INT;
BEGIN
    INSERT INTO Fine (
        FineID, ReaderID, BorrowRecordID, FineType, Amount, 
        DueDate, PayStatus, Remark
    ) VALUES (
        seq_fine_id.NEXTVAL, 
        :NEW.ReaderID, 
        :NEW.BorrowRecordID, 
        '逾期罚款', 
        :NEW.OverdueFine,
        SYSTIMESTAMP + 30,  -- 30天内缴清
        '未缴纳',
        '系统自动生成的逾期罚款，逾期天数：' || (:NEW.OverdueFine * 2) || '天'
    )
    RETURNING FineID INTO v_fine_id;

    -- 发送罚款通知
    INSERT INTO Notification (
        NotificationID, ReaderID, Title, Content, Type,
        RelatedType, RelatedID, SenderType, CreateTime, Priority
    ) VALUES (
        seq_notif_id.NEXTVAL, 
        :NEW.ReaderID,
        '逾期罚款生成通知',
        '您因逾期归还图书产生罚款' || :NEW.OverdueFine || '元，请在30天内前往图书馆缴纳。',
        '罚款通知',
        'Fine', v_fine_id,
        '系统', SYSTIMESTAMP, '重要'
    );

    -- 记录操作日志
    INSERT INTO OperationLog (
        LogID, OperationTime, OperatorID, OperatorType,
        OperationModule, OperationAction, TargetType, TargetID,
        OperationDetail, Status
    ) VALUES (
        seq_log_id.NEXTVAL, SYSTIMESTAMP, 'SYSTEM', '系统',
        '罚款管理', '生成罚款', 'Fine', TO_CHAR(v_fine_id),
        '自动生成逾期罚款：读者' || :NEW.ReaderID || '归还图书，逾期罚款' || :NEW.OverdueFine || '元',
        '成功'
    );
END;
/

-- ================================================================
-- 预约过期定时清理触发器
-- 当预约记录过期时自动更新状态
-- ================================================================
CREATE OR REPLACE TRIGGER trg_expire_reservation
BEFORE UPDATE ON ReserveBook
FOR EACH ROW
WHEN (OLD.Status = '等待中' AND OLD.ExpireTime < SYSTIMESTAMP)
BEGIN
    :NEW.Status := '已过期';
END;
/

-- ================================================================
-- 通知自动清理触发器
-- 通知过期后自动标记为已过期
-- ================================================================
CREATE OR REPLACE TRIGGER trg_expire_notification
BEFORE UPDATE ON Notification
FOR EACH ROW
WHEN (OLD.ExpireTime IS NOT NULL AND OLD.ExpireTime < SYSTIMESTAMP AND OLD.Status = '有效')
BEGIN
    :NEW.Status := '已过期';
END;
/

-- ================================================================
-- 读者借阅数量约束触发器
-- 保证读者未归还图书数量不超过上限
-- ================================================================
CREATE OR REPLACE TRIGGER trg_check_borrow_limit
BEFORE INSERT ON BorrowRecord
FOR EACH ROW
DECLARE
    v_current_borrow INT;
    v_max_borrow INT := 5;  -- 最大借阅数量
BEGIN
    SELECT COUNT(*) INTO v_current_borrow
    FROM BorrowRecord
    WHERE ReaderID = :NEW.ReaderID
      AND ReturnTime IS NULL;

    IF v_current_borrow >= v_max_borrow THEN
        RAISE_APPLICATION_ERROR(-20001, '已达最大借阅数量（' || v_max_borrow || '本），请先归还部分图书');
    END IF;
END;
/

-- ================================================================
-- 评论删除级联通知触发器
-- 当评论被删除时，同步处理相关举报
-- ================================================================
CREATE OR REPLACE TRIGGER trg_cascade_comment_delete
AFTER UPDATE ON Comment_Table
FOR EACH ROW
WHEN (OLD.Status = '正常' AND NEW.Status = '已删除')
BEGIN
    -- 自动驳回相关的举报
    UPDATE Report
    SET Status = '驳回',
        HandleResult = CONCAT(NVL(HandleResult, ''), ' | 评论已删除，举报自动驳回')
    WHERE CommentID = :NEW.CommentID
      AND Status = '待处理';
END;
/
