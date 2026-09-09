-- ================================================================
-- 预约超时定时任务
-- 处理已过期未借出的预约
-- ================================================================

-- 1. 创建存储过程：清理过期预约
CREATE OR REPLACE PROCEDURE process_expired_reservations AS
    v_count NUMBER := 0;
BEGIN
    -- 将过期的预约标记为已过期
    UPDATE ReserveBook
    SET Status = '已过期'
    WHERE Status = '已通知'
      AND ExpireTime < SYSTIMESTAMP;

    v_count := SQL%ROWCOUNT;

    -- 记录日志
    INSERT INTO SystemLog (LogType, LogContent, LogTime)
    VALUES ('预约清理', '清理过期预约 ' || v_count || ' 条', SYSTIMESTAMP);

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('清理过期预约完成，共处理 ' || v_count || ' 条');
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        DBMS_OUTPUT.PUT_LINE('清理失败: ' || SQLERRM);
END;
/

-- 2. 创建存储过程：发送预约到期提醒（预约还有1天到期）
CREATE OR REPLACE PROCEDURE send_reservation_expiry_reminders AS
    v_count NUMBER := 0;
BEGIN
    -- 找出1天内到期的预约，发送提醒通知
    FOR rec IN (
        SELECT r.ReserveID, r.ReaderID, r.ISBN, r.ExpireTime,
               bi.Title AS BookTitle
        FROM ReserveBook r
        JOIN BookInfo bi ON r.ISBN = bi.ISBN
        WHERE r.Status = '已通知'
          AND r.ExpireTime BETWEEN SYSTIMESTAMP AND SYSTIMESTAMP + INTERVAL '1' DAY
          AND NOT EXISTS (
              SELECT 1 FROM Notification n
              WHERE n.RelatedID = r.ReserveID
                AND n.Type = '预约到期提醒'
                AND n.CreateTime > SYSTIMESTAMP - INTERVAL '1' DAY
          )
    ) LOOP
        INSERT INTO Notification (NotificationID, ReaderID, Title, Content, Type, Priority, IsRead, CreateTime, RelatedID)
        VALUES (
            seq_notification_id.NEXTVAL,
            rec.ReaderID,
            '预约即将到期',
            '您预约的《' || rec.BookTitle || '》将于 ' || TO_CHAR(rec.ExpireTime, 'YYYY-MM-DD HH24:MI:SS') || ' 到期，请尽快到馆借阅。',
            '预约到期提醒',
            '重要',
            'N',
            SYSTIMESTAMP,
            rec.ReserveID
        );
        v_count := v_count + 1;
    END LOOP;

    -- 记录日志
    INSERT INTO SystemLog (LogType, LogContent, LogTime)
    VALUES ('预约提醒', '发送预约到期提醒 ' || v_count || ' 条', SYSTIMESTAMP);

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('发送预约到期提醒完成，共处理 ' || v_count || ' 条');
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        DBMS_OUTPUT.PUT_LINE('发送提醒失败: ' || SQLERRM);
END;
/

-- 3. 创建存储过程：自动处理逾期罚款（已有，此处仅做引用）
-- 调用 process_overdue 存储过程（已有）

-- 4. 创建存储过程：自动发送逾期提醒（图书到期前3天提醒）
CREATE OR REPLACE PROCEDURE send_overdue_reminders AS
    v_count NUMBER := 0;
BEGIN
    -- 找出3天内到期的借阅记录
    FOR rec IN (
        SELECT br.BorrowRecordID, br.ReaderID, br.DueTime,
               bi.Title AS BookTitle
        FROM BorrowRecord br
        JOIN Book b ON br.BookID = b.BookID
        JOIN BookInfo bi ON b.ISBN = bi.ISBN
        WHERE br.ReturnTime IS NULL
          AND br.DueTime BETWEEN SYSTIMESTAMP AND SYSTIMESTAMP + INTERVAL '3' DAY
          AND NOT EXISTS (
              SELECT 1 FROM Notification n
              WHERE n.RelatedID = br.BorrowRecordID
                AND n.Type = '逾期提醒'
                AND n.CreateTime > SYSTIMESTAMP - INTERVAL '3' DAY
          )
    ) LOOP
        INSERT INTO Notification (NotificationID, ReaderID, Title, Content, Type, Priority, IsRead, CreateTime, RelatedID)
        VALUES (
            seq_notification_id.NEXTVAL,
            rec.ReaderID,
            '借阅即将到期',
            '您借阅的《' || rec.BookTitle || '》将于 ' || TO_CHAR(rec.DueTime, 'YYYY-MM-DD HH24:MI:SS') || ' 到期，请及时归还。',
            '逾期提醒',
            '重要',
            'N',
            SYSTIMESTAMP,
            rec.BorrowRecordID
        );
        v_count := v_count + 1;
    END LOOP;

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('发送逾期提醒完成，共处理 ' || v_count || ' 条');
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        DBMS_OUTPUT.PUT_LINE('发送逾期提醒失败: ' || SQLERRM);
END;
/

-- 5. 创建定时任务调度器（需要DBMS_SCHEDULER权限）
-- 注意：执行需要管理员权限

-- 每天凌晨2点清理过期预约
BEGIN
    DBMS_SCHEDULER.CREATE_JOB(
        job_name        => 'JOB_CLEAN_EXPIRED_RESERVATIONS',
        job_type        => 'PLSQL_BLOCK',
        job_action      => 'BEGIN process_expired_reservations; END;',
        start_date      => SYSTIMESTAMP,
        repeat_interval => 'FREQ=DAILY; BYHOUR=2; BYMINUTE=0',
        enabled         => TRUE,
        auto_drop       => FALSE,
        comments        => '每天凌晨2点清理过期预约'
    );
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('创建定时任务失败: ' || SQLERRM);
END;
/

-- 每天上午9点发送预约到期提醒
BEGIN
    DBMS_SCHEDULER.CREATE_JOB(
        job_name        => 'JOB_RESERVATION_REMINDERS',
        job_type        => 'PLSQL_BLOCK',
        job_action      => 'BEGIN send_reservation_expiry_reminders; END;',
        start_date      => SYSTIMESTAMP,
        repeat_interval => 'FREQ=DAILY; BYHOUR=9; BYMINUTE=0',
        enabled         => TRUE,
        auto_drop       => FALSE,
        comments        => '每天上午9点发送预约到期提醒'
    );
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('创建定时任务失败: ' || SQLERRM);
END;
/

-- 每天上午9点发送逾期提醒
BEGIN
    DBMS_SCHEDULER.CREATE_JOB(
        job_name        => 'JOB_OVERDUE_REMINDERS',
        job_type        => 'PLSQL_BLOCK',
        job_action      => 'BEGIN send_overdue_reminders; END;',
        start_date      => SYSTIMESTAMP,
        repeat_interval => 'FREQ=DAILY; BYHOUR=9; BYMINUTE=0',
        enabled         => TRUE,
        auto_drop       => FALSE,
        comments        => '每天上午9点发送逾期提醒'
    );
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('创建定时任务失败: ' || SQLERRM);
END;
/

-- 查看已创建的定时任务
-- SELECT job_name, job_action, repeat_interval, enabled FROM USER_SCHEDULER_JOBS WHERE job_name LIKE 'JOB_%';
