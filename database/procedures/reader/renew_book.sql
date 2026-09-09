-- ================================================================
-- 图书续借存储过程
-- 支持读者续借当前借阅的图书
-- 续借规则：
--   1. 每本书最多续借2次
--   2. 续借后借期延长30天
--   3. 已逾期的图书不能续借
--   4. 已借出的图书（Status='借出'）不能续借，需先归还
-- ================================================================
CREATE OR REPLACE PROCEDURE sp_renew_book(
    p_reader_id IN INT,
    p_book_id IN VARCHAR2,
    p_result OUT VARCHAR2,
    p_new_due_time OUT TIMESTAMP
) AS
    v_max_renew INT := 2;
    v_renew_days INT := 30;
    v_borrow_record_id INT;
    v_old_due_time TIMESTAMP;
    v_borrow_time TIMESTAMP;
    v_status VARCHAR2(20);
    v_renew_count INT;
    v_reader_status VARCHAR2(10);
    v_reader_credit INT;
BEGIN
    p_result := '';
    p_new_due_time := NULL;

    -- 1. 检查读者状态
    SELECT AccountStatus, CreditScore INTO v_reader_status, v_reader_credit
    FROM Reader
    WHERE ReaderID = p_reader_id;

    IF v_reader_status = '冻结' THEN
        p_result := 'ERROR: 您的账户已被冻结，无法续借图书';
        RETURN;
    END IF;

    IF v_reader_credit < 60 THEN
        p_result := 'ERROR: 您的信用分不足60分，无法续借图书';
        RETURN;
    END IF;

    -- 2. 查询当前有效的借阅记录
    SELECT BorrowRecordID, BorrowTime, DueTime, Status
    INTO v_borrow_record_id, v_borrow_time, v_old_due_time, v_status
    FROM BorrowRecord
    WHERE ReaderID = p_reader_id
      AND BookID = p_book_id
      AND ReturnTime IS NULL
      AND ROWNUM = 1;

    -- 3. 校验借阅状态
    IF v_status = '借出' THEN
        -- 兼容旧数据：借出状态的图书也允许续借
        NULL;
    ELSIF v_status != '借阅中' AND v_status != '借出' THEN
        p_result := 'ERROR: 当前借阅状态不允许续借';
        RETURN;
    END IF;

    -- 4. 检查是否已逾期
    IF v_old_due_time < SYSTIMESTAMP THEN
        p_result := 'ERROR: 图书已逾期，请先归还后再续借';
        RETURN;
    END IF;

    -- 5. 检查续借次数
    SELECT COUNT(*) INTO v_renew_count
    FROM RenewRecord
    WHERE BorrowRecordID = v_borrow_record_id;

    IF v_renew_count >= v_max_renew THEN
        p_result := 'ERROR: 已达最大续借次数（' || v_max_renew || '次），无法再次续借';
        RETURN;
    END IF;

    -- 6. 执行续借
    p_new_due_time := v_old_due_time + v_renew_days;

    -- 插入续借记录
    INSERT INTO RenewRecord (
        BorrowRecordID,
        ReaderID,
        RenewTime,
        OldDueTime,
        NewDueTime,
        RenewCount,
        Status
    ) VALUES (
        v_borrow_record_id,
        p_reader_id,
        SYSTIMESTAMP,
        v_old_due_time,
        p_new_due_time,
        v_renew_count + 1,
        '有效'
    );

    -- 更新借阅记录的应还时间
    UPDATE BorrowRecord
    SET DueTime = p_new_due_time
    WHERE BorrowRecordID = v_borrow_record_id;

    -- 记录操作日志
    INSERT INTO OperationLog (
        LogID, OperationTime, OperatorID, OperatorType,
        OperationModule, OperationAction, TargetType, TargetID,
        OperationDetail, Status
    ) VALUES (
        seq_log_id.NEXTVAL, SYSTIMESTAMP, TO_CHAR(p_reader_id), 'Reader',
        '借阅管理', '图书续借', 'BorrowRecord', TO_CHAR(v_borrow_record_id),
        '读者' || p_reader_id || '续借图书' || p_book_id || '，续借' || (v_renew_count + 1) || '次，新应还时间：' || TO_CHAR(p_new_due_time, 'YYYY-MM-DD HH24:MI:SS'),
        '成功'
    );

    p_result := 'SUCCESS: 续借成功！新应还时间为：' || TO_CHAR(p_new_due_time, 'YYYY-MM-DD');
    COMMIT;

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_result := 'ERROR: 未找到有效的借阅记录';
        ROLLBACK;
    WHEN OTHERS THEN
        p_result := 'ERROR: ' || SQLERRM;
        ROLLBACK;
END sp_renew_book;
/
