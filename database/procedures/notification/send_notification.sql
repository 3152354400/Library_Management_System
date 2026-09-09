-- ================================================================
-- 查询读者消息通知列表
-- ================================================================
CREATE OR REPLACE PROCEDURE sp_get_reader_notifications(
    p_reader_id IN INT,
    p_is_read IN VARCHAR2 DEFAULT NULL,
    p_type IN VARCHAR2 DEFAULT NULL,
    p_page_size IN INT DEFAULT 20,
    p_page_num IN INT DEFAULT 1,
    p_result OUT SYS_REFCURSOR
) AS
    v_offset INT;
BEGIN
    v_offset := (p_page_num - 1) * p_page_size;

    OPEN p_result FOR
    SELECT 
        NotificationID,
        Title,
        Content,
        Type,
        Priority,
        CreateTime,
        IsRead,
        ReadTime,
        RelatedType,
        RelatedID
    FROM Notification
    WHERE ReaderID = p_reader_id
      AND Status = '有效'
      AND (p_is_read IS NULL OR IsRead = p_is_read)
      AND (p_type IS NULL OR Type = p_type)
    ORDER BY CreateTime DESC
    OFFSET v_offset ROWS FETCH NEXT p_page_size ROWS ONLY;
END sp_get_reader_notifications;
/

-- ================================================================
-- 获取未读通知数量
-- ================================================================
CREATE OR REPLACE PROCEDURE sp_get_unread_notification_count(
    p_reader_id IN INT,
    p_count OUT INT
) AS
BEGIN
    SELECT COUNT(*)
    INTO p_count
    FROM Notification
    WHERE ReaderID = p_reader_id
      AND IsRead = 'N'
      AND Status = '有效';
END sp_get_unread_notification_count;
/

-- ================================================================
-- 标记通知为已读
-- ================================================================
CREATE OR REPLACE PROCEDURE sp_mark_notification_read(
    p_notification_id IN INT,
    p_reader_id IN INT,
    p_result OUT VARCHAR2
) AS
    v_current_reader INT;
BEGIN
    -- 验证归属
    SELECT ReaderID INTO v_current_reader
    FROM Notification
    WHERE NotificationID = p_notification_id;

    IF v_current_reader != p_reader_id THEN
        p_result := 'ERROR: 无权操作他人的通知';
        RETURN;
    END IF;

    UPDATE Notification
    SET IsRead = 'Y',
        ReadTime = SYSTIMESTAMP
    WHERE NotificationID = p_notification_id;

    p_result := 'SUCCESS';
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_result := 'ERROR: 通知不存在';
END sp_mark_notification_read;
/

-- ================================================================
-- 批量标记通知为已读
-- ================================================================
CREATE OR REPLACE PROCEDURE sp_mark_all_notifications_read(
    p_reader_id IN INT,
    p_result OUT INT
) AS
BEGIN
    UPDATE Notification
    SET IsRead = 'Y',
        ReadTime = SYSTIMESTAMP
    WHERE ReaderID = p_reader_id
      AND IsRead = 'N'
      AND Status = '有效';

    p_result := SQL%ROWCOUNT;
END sp_mark_all_notifications_read;
/

-- ================================================================
-- 删除通知
-- ================================================================
CREATE OR REPLACE PROCEDURE sp_delete_notification(
    p_notification_id IN INT,
    p_reader_id IN INT,
    p_result OUT VARCHAR2
) AS
    v_current_reader INT;
BEGIN
    SELECT ReaderID INTO v_current_reader
    FROM Notification
    WHERE NotificationID = p_notification_id;

    IF v_current_reader != p_reader_id THEN
        p_result := 'ERROR: 无权删除他人的通知';
        RETURN;
    END IF;

    UPDATE Notification
    SET Status = '已删除'
    WHERE NotificationID = p_notification_id;

    p_result := 'SUCCESS';
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_result := 'ERROR: 通知不存在';
END sp_delete_notification;
/

-- ================================================================
-- 发送逾期提醒（定时任务调用）
-- 查找即将逾期（3天内）和已逾期的借阅记录，发送通知
-- ================================================================
CREATE OR REPLACE PROCEDURE sp_send_overdue_reminder(
    p_days_before INT DEFAULT 3,
    p_result OUT VARCHAR2
) AS
    v_count INT := 0;
    v_reader_id INT;
    v_book_id VARCHAR2(20);
    v_due_time TIMESTAMP;
    v_book_title VARCHAR2(200);
BEGIN
    p_result := '';

    -- 1. 发送即将逾期提醒（3天内）
    FOR rec IN (
        SELECT br.ReaderID, br.BookID, br.DueTime, bi.Title
        FROM BorrowRecord br
        JOIN Book b ON br.BookID = b.BookID
        JOIN BookInfo bi ON b.ISBN = bi.ISBN
        WHERE br.Status IN ('借阅中', '借出')
          AND br.ReturnTime IS NULL
          AND br.DueTime BETWEEN SYSTIMESTAMP AND SYSTIMESTAMP + p_days_before
    ) LOOP
        -- 检查是否已发送过提醒
        SELECT COUNT(*) INTO v_count
        FROM Notification
        WHERE ReaderID = rec.ReaderID
          AND RelatedType = 'BorrowRecord'
          AND RelatedID = rec.BookID
          AND Type = '逾期提醒'
          AND CreateTime > SYSTIMESTAMP - 1;  -- 1天内不重复发送

        IF v_count = 0 THEN
            INSERT INTO Notification (
                NotificationID, ReaderID, Title, Content, Type,
                RelatedType, RelatedID, SenderType, CreateTime, Priority
            ) VALUES (
                seq_notif_id.NEXTVAL, rec.ReaderID,
                '图书即将逾期提醒',
                '您借阅的图书《' || rec.Title || '》将于' || 
                TO_CHAR(rec.DueTime, 'YYYY-MM-DD') || '到期，请及时归还或续借！',
                '逾期提醒',
                'BorrowRecord', rec.BookID,
                '系统', SYSTIMESTAMP, '重要'
            );
        END IF;
    END LOOP;

    -- 2. 发送已逾期通知（首次逾期当天）
    FOR rec IN (
        SELECT br.ReaderID, br.BookID, br.DueTime, bi.Title
        FROM BorrowRecord br
        JOIN Book b ON br.BookID = b.BookID
        JOIN BookInfo bi ON b.ISBN = bi.ISBN
        WHERE br.Status IN ('借阅中', '借出')
          AND br.ReturnTime IS NULL
          AND br.DueTime < SYSTIMESTAMP
          AND TRUNC(br.DueTime) = TRUNC(SYSTIMESTAMP - 1)  -- 昨天刚逾期
    ) LOOP
        -- 检查是否已发送过
        SELECT COUNT(*) INTO v_count
        FROM Notification
        WHERE ReaderID = rec.ReaderID
          AND RelatedType = 'OverdueBorrow'
          AND RelatedID = rec.BookID
          AND CreateTime > SYSTIMESTAMP - 1;

        IF v_count = 0 THEN
            INSERT INTO Notification (
                NotificationID, ReaderID, Title, Content, Type,
                RelatedType, RelatedID, SenderType, CreateTime, Priority
            ) VALUES (
                seq_notif_id.NEXTVAL, rec.ReaderID,
                '图书已逾期通知',
                '您借阅的图书《' || rec.Title || '》已超过应还日期' || 
                TRUNC(SYSTIMESTAMP - rec.DueTime) || '天，请尽快归还！逾期将产生罚款并影响信用分。',
                '逾期提醒',
                'OverdueBorrow', rec.BookID,
                '系统', SYSTIMESTAMP, '紧急'
            );
        END IF;
    END LOOP;

    -- 3. 记录操作日志
    INSERT INTO OperationLog (
        LogID, OperationTime, OperatorID, OperatorType,
        OperationModule, OperationAction, OperationDetail, Status
    ) VALUES (
        seq_log_id.NEXTVAL, SYSTIMESTAMP, 'SYSTEM', '系统',
        '提醒管理', '发送逾期提醒', '系统自动执行逾期提醒任务', '成功'
    );

    p_result := 'SUCCESS: 逾期提醒任务执行完成';
    COMMIT;

EXCEPTION
    WHEN OTHERS THEN
        p_result := 'ERROR: ' || SQLERRM;
        ROLLBACK;
END sp_send_overdue_reminder;
/

-- ================================================================
-- 预约到书通知（管理员还书时调用）
-- 当读者归还图书时，通知等待预约的读者
-- ================================================================
CREATE OR REPLACE PROCEDURE sp_notify_reserve_available(
    p_isbn IN VARCHAR2,
    p_result OUT VARCHAR2
) AS
    v_book_title VARCHAR2(200);
    v_reserve_count INT := 0;
BEGIN
    p_result := '';

    -- 获取书名
    SELECT Title INTO v_book_title
    FROM BookInfo
    WHERE ISBN = p_isbn;

    -- 查找等待预约的读者（按预约时间排序）
    FOR rec IN (
        SELECT ReserveID, ReaderID
        FROM ReserveBook
        WHERE ISBN = p_isbn
          AND Status = '等待中'
          AND ExpireTime > SYSTIMESTAMP
        ORDER BY ReserveTime ASC
    ) LOOP
        -- 更新预约状态为已通知
        UPDATE ReserveBook
        SET Status = '已通知',
            NotifyTime = SYSTIMESTAMP,
            HandleTime = SYSTIMESTAMP
        WHERE ReserveID = rec.ReserveID;

        -- 发送通知
        INSERT INTO Notification (
            NotificationID, ReaderID, Title, Content, Type,
            RelatedType, RelatedID, SenderType, CreateTime, Priority
        ) VALUES (
            seq_notif_id.NEXTVAL, rec.ReaderID,
            '预约图书已到店',
            '您预约的图书《' || v_book_title || '》已有可借副本，请到馆办理借阅。预约保留至' || 
            TO_CHAR((SELECT ExpireTime FROM ReserveBook WHERE ReserveID = rec.ReserveID), 'YYYY-MM-DD') || '。',
            '预约到书',
            'ReserveBook', rec.ReserveID,
            '系统', SYSTIMESTAMP, '紧急'
        );

        v_reserve_count := v_reserve_count + 1;

        -- 只通知第一个预约者
        EXIT;
    END LOOP;

    p_result := 'SUCCESS: 已通知' || v_reserve_count || '位预约读者';
END sp_notify_reserve_available;
/
