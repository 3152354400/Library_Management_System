-- ================================================================
-- 处理所有逾期记录存储过程
-- 功能：
--   1. 查找所有已逾期但未归还的借阅记录
--   2. 更新借阅状态为"逾期"
--   3. 计算并更新逾期罚款
--   4. 扣减读者信用分（首次逾期扣5分，后续扣10分）
--   5. 信用分低于40分时自动冻结账户
--   6. 发送逾期通知
-- 调度：建议每日凌晨2点执行
-- ================================================================
CREATE OR REPLACE PROCEDURE ProcessOverdueRecords AS
    -- 游标：查询所有未归还且已逾期的记录
    CURSOR c_overdue IS
        SELECT
            br.BorrowRecordID,
            br.ReaderID,
            br.BookID,
            br.BorrowTime,
            br.DueTime,
            r.CreditScore,
            r.AccountStatus,
            br.IsFirstOverdue,
            br.OverdueCount
        FROM BorrowRecord br
        JOIN Reader r ON br.ReaderID = r.ReaderID
        WHERE br.ReturnTime IS NULL  -- 未归还
          AND br.Status IN ('借阅中', '借出')  -- 借阅中或借出状态
          AND br.DueTime < TRUNC(SYSDATE);  -- 已超过应还时间

    v_fine NUMBER(10,2);       -- 计算的罚款
    v_credit_deduct INT;       -- 信用分扣减额
    v_days_overdue INT;        -- 逾期天数
    v_processed_count INT := 0; -- 已处理记录数
    v_book_title VARCHAR2(200); -- 图书标题
BEGIN
    FOR rec IN c_overdue LOOP
        -- 1. 计算逾期天数
        v_days_overdue := TRUNC(SYSDATE) - TRUNC(rec.DueTime);

        -- 2. 计算逾期罚款
        v_fine := CalculateOverdueFine(rec.BorrowTime, NULL);

        -- 3. 更新借阅记录的逾期状态和罚款
        UPDATE BorrowRecord
        SET Status = '逾期',
            OverdueFine = v_fine
        WHERE BorrowRecordID = rec.BorrowRecordID;

        -- 4. 计算信用分扣减（首次逾期扣5分，后续扣10分）
        IF rec.IsFirstOverdue = 1 OR rec.IsFirstOverdue IS NULL THEN
            v_credit_deduct := 5;
            -- 标记为非首次逾期
            UPDATE BorrowRecord
            SET IsFirstOverdue = 0
            WHERE BorrowRecordID = rec.BorrowRecordID;
        ELSE
            v_credit_deduct := 10;
        END IF;

        -- 5. 更新读者信用分（扣除扣减额，最低为0）
        UPDATE Reader
        SET CreditScore = GREATEST(CreditScore - v_credit_deduct, 0),
            OverdueCount = NVL(OverdueCount, 0) + 1
        WHERE ReaderID = rec.ReaderID;

        -- 6. 若信用分低于40，冻结账户
        UPDATE Reader
        SET AccountStatus = '冻结'
        WHERE ReaderID = rec.ReaderID
          AND CreditScore < 40;

        -- 7. 发送逾期通知
        BEGIN
            SELECT bi.Title INTO v_book_title
            FROM Book b
            JOIN BookInfo bi ON b.ISBN = bi.ISBN
            WHERE b.BookID = rec.BookID;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                v_book_title := '未知图书';
        END;

        INSERT INTO Notification (
            NotificationID, ReaderID, Title, Content, Type,
            RelatedType, RelatedID, SenderType, CreateTime, Priority
        ) VALUES (
            seq_notif_id.NEXTVAL, rec.ReaderID,
            '图书逾期通知',
            '您借阅的图书《' || v_book_title || '》已逾期' || v_days_overdue || '天，请尽快归还！' ||
            '逾期罚款：' || v_fine || '元。连续逾期将影响您的信用分。',
            '逾期提醒',
            'BorrowRecord', rec.BorrowRecordID,
            '系统', SYSTIMESTAMP, '紧急'
        );

        -- 8. 记录操作日志
        INSERT INTO OperationLog (
            LogID, OperationTime, OperatorID, OperatorType,
            OperationModule, OperationAction, TargetType, TargetID,
            OperationDetail, Status
        ) VALUES (
            seq_log_id.NEXTVAL, SYSTIMESTAMP, 'SYSTEM', '系统',
            '逾期管理', '处理逾期', 'BorrowRecord', TO_CHAR(rec.BorrowRecordID),
            '自动处理逾期：读者' || rec.ReaderID || '借阅图书' || rec.BookID ||
            '，逾期' || v_days_overdue || '天，罚款' || v_fine || '元，扣信用分' || v_credit_deduct || '分',
            '成功'
        );

        v_processed_count := v_processed_count + 1;
    END LOOP;

    -- 记录任务执行日志
    INSERT INTO OperationLog (
        LogID, OperationTime, OperatorID, OperatorType,
        OperationModule, OperationAction, OperationDetail, Status
    ) VALUES (
        seq_log_id.NEXTVAL, SYSTIMESTAMP, 'SYSTEM', '系统',
        '逾期管理', '逾期任务执行', '本次共处理' || v_processed_count || '条逾期记录', '成功'
    );

    COMMIT;

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END ProcessOverdueRecords;
/
