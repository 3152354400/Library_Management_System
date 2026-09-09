-- ================================================================
-- 预约图书存储过程
-- 支持读者预约当前无可借副本的图书
-- ================================================================
CREATE OR REPLACE PROCEDURE sp_reserve_book(
    p_reader_id IN INT,
    p_isbn IN VARCHAR2,
    p_expected_days IN INT DEFAULT 7,
    p_result OUT VARCHAR2,
    p_reserve_id OUT INT
) AS
    v_available_count INT;
    v_already_reserved INT;
    v_already_borrowed INT;
    v_reader_status VARCHAR2(10);
    v_max_reserve INT := 3;
    v_current_reserve_count INT;
BEGIN
    p_result := '';
    p_reserve_id := 0;

    -- 1. 检查读者状态
    SELECT AccountStatus INTO v_reader_status
    FROM Reader
    WHERE ReaderID = p_reader_id;

    IF v_reader_status = '冻结' THEN
        p_result := 'ERROR: 您的账户已被冻结，无法预约图书';
        RETURN;
    END IF;

    -- 2. 检查读者预约数量限制
    SELECT COUNT(*)
    INTO v_current_reserve_count
    FROM ReserveBook
    WHERE ReaderID = p_reader_id
      AND Status IN ('等待中', '已通知');

    IF v_current_reserve_count >= v_max_reserve THEN
        p_result := 'ERROR: 您已有' || v_current_reserve_count || '个有效预约，已达上限（' || v_max_reserve || '个）';
        RETURN;
    END IF;

    -- 3. 检查图书是否存在
    SELECT AvailableStock INTO v_available_count
    FROM BookInfo
    WHERE ISBN = p_isbn;

    IF v_available_count > 0 THEN
        p_result := 'HINT: 该图书当前有可借副本，建议直接到馆借阅';
        RETURN;
    END IF;

    -- 4. 检查是否已预约过该图书
    SELECT COUNT(*)
    INTO v_already_reserved
    FROM ReserveBook
    WHERE ReaderID = p_reader_id
      AND ISBN = p_isbn
      AND Status IN ('等待中', '已通知');

    IF v_already_reserved > 0 THEN
        p_result := 'ERROR: 您已经预约过该图书，请勿重复预约';
        RETURN;
    END IF;

    -- 5. 检查是否已借阅该图书
    SELECT COUNT(*)
    INTO v_already_borrowed
    FROM BorrowRecord br
    JOIN Book b ON br.BookID = b.BookID
    WHERE br.ReaderID = p_reader_id
      AND b.ISBN = p_isbn
      AND br.ReturnTime IS NULL;

    IF v_already_borrowed > 0 THEN
        p_result := 'ERROR: 您已借阅该图书，请先归还后再预约';
        RETURN;
    END IF;

    -- 6. 创建预约记录
    INSERT INTO ReserveBook (
        ReaderID,
        ISBN,
        ReserveTime,
        ExpectedBorrowTime,
        ExpireTime,
        Status,
        Priority
    ) VALUES (
        p_reader_id,
        p_isbn,
        SYSTIMESTAMP,
        SYSTIMESTAMP + p_expected_days,
        SYSTIMESTAMP + p_expected_days + 3,  -- 预约成功后保留3天
        '等待中',
        0
    )
    RETURNING ReserveID INTO p_reserve_id;

    -- 7. 发送预约成功通知
    INSERT INTO Notification (
        NotificationID, ReaderID, Title, Content, Type,
        RelatedType, RelatedID, SenderType, CreateTime
    ) VALUES (
        seq_notif_id.NEXTVAL, p_reader_id,
        '图书预约成功',
        '您已成功预约图书《' || 
        (SELECT Title FROM BookInfo WHERE ISBN = p_isbn) || 
        '》，请耐心等待。当有读者归还该书时，系统将通知您取书。',
        '预约到书',
        'ReserveBook', p_reserve_id,
        '系统', SYSTIMESTAMP
    );

    -- 8. 记录操作日志
    INSERT INTO OperationLog (
        LogID, OperationTime, OperatorID, OperatorType,
        OperationModule, OperationAction, TargetType, TargetID,
        OperationDetail, Status
    ) VALUES (
        seq_log_id.NEXTVAL, SYSTIMESTAMP, TO_CHAR(p_reader_id), 'Reader',
        '预约管理', '预约图书', 'BookInfo', p_isbn,
        '读者' || p_reader_id || '预约图书ISBN:' || p_isbn || '，预约编号：' || p_reserve_id,
        '成功'
    );

    p_result := 'SUCCESS: 预约成功！预约编号：' || p_reserve_id || '，请关注通知消息';
    COMMIT;

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_result := 'ERROR: 图书不存在';
        ROLLBACK;
    WHEN OTHERS THEN
        p_result := 'ERROR: ' || SQLERRM;
        ROLLBACK;
END sp_reserve_book;
/

-- ================================================================
-- 取消预约存储过程
-- ================================================================
CREATE OR REPLACE PROCEDURE sp_cancel_reserve(
    p_reserve_id IN INT,
    p_reader_id IN INT,
    p_cancel_reason IN VARCHAR2 DEFAULT NULL,
    p_result OUT VARCHAR2
) AS
    v_status VARCHAR2(20);
    v_current_reader INT;
BEGIN
    p_result := '';

    -- 1. 检查预约是否存在且属于该读者
    SELECT Status, ReaderID INTO v_status, v_current_reader
    FROM ReserveBook
    WHERE ReserveID = p_reserve_id;

    IF v_current_reader != p_reader_id THEN
        p_result := 'ERROR: 无权取消他人的预约';
        RETURN;
    END IF;

    -- 2. 检查状态
    IF v_status NOT IN ('等待中', '已通知') THEN
        p_result := 'ERROR: 当前预约状态不允许取消';
        RETURN;
    END IF;

    -- 3. 更新预约状态
    UPDATE ReserveBook
    SET Status = '已取消',
        CancelReason = p_cancel_reason,
        HandleTime = SYSTIMESTAMP
    WHERE ReserveID = p_reserve_id;

    -- 4. 记录操作日志
    INSERT INTO OperationLog (
        LogID, OperationTime, OperatorID, OperatorType,
        OperationModule, OperationAction, TargetType, TargetID,
        OperationDetail, Status
    ) VALUES (
        seq_log_id.NEXTVAL, SYSTIMESTAMP, TO_CHAR(p_reader_id), 'Reader',
        '预约管理', '取消预约', 'ReserveBook', TO_CHAR(p_reserve_id),
        '读者' || p_reader_id || '取消预约，编号：' || p_reserve_id || '，原因：' || NVL(p_cancel_reason, '用户主动取消'),
        '成功'
    );

    p_result := 'SUCCESS: 预约已取消';
    COMMIT;

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_result := 'ERROR: 预约记录不存在';
        ROLLBACK;
    WHEN OTHERS THEN
        p_result := 'ERROR: ' || SQLERRM;
        ROLLBACK;
END sp_cancel_reserve;
/

-- ================================================================
-- 查询读者预约列表
-- ================================================================
CREATE OR REPLACE PROCEDURE sp_get_reader_reserves(
    p_reader_id IN INT,
    p_result OUT SYS_REFCURSOR
) AS
BEGIN
    OPEN p_result FOR
    SELECT 
        rb.ReserveID,
        rb.ISBN,
        bi.Title AS BookTitle,
        bi.Author,
        rb.ReserveTime,
        rb.ExpectedBorrowTime,
        rb.ExpireTime,
        rb.Status,
        rb.Remark
    FROM ReserveBook rb
    JOIN BookInfo bi ON rb.ISBN = bi.ISBN
    WHERE rb.ReaderID = p_reader_id
    ORDER BY rb.ReserveTime DESC;
END sp_get_reader_reserves;
/
