/* =========================================================
   三人版图书馆管理系统 - 存储过程脚本
   Database: Oracle 19c
   Depends on:
   - schema_three_person.sql
   - functions_three_person.sql
   - triggers_three_person.sql
   ========================================================= */


/* =========================================================
   1. borrow_book
   功能：读者借书。
   业务规则：
   - 读者账户必须正常
   - 读者信用分不得低于 60
   - 当前未归还图书数量不得超过 5 本
   - 实体图书状态必须为“正常”
   - 借书成功后生成借阅记录，图书状态改为“借出”
   ========================================================= */
CREATE OR REPLACE PROCEDURE borrow_book (
    p_reader_id IN INT,
    p_book_id IN INT
)
IS
    v_reader_status VARCHAR2(10);
    v_credit INT;
    v_book_status VARCHAR2(10);
    v_unreturned_count INT;
BEGIN
    SELECT AccountStatus, CreditScore
    INTO v_reader_status, v_credit
    FROM Reader
    WHERE ReaderID = p_reader_id;

    IF v_reader_status <> '正常' THEN
        RAISE_APPLICATION_ERROR(-20001, '读者账户状态异常，不能借书');
    END IF;

    IF v_credit < 60 THEN
        RAISE_APPLICATION_ERROR(-20002, '信用分低于60，不能借书');
    END IF;

    SELECT COUNT(*)
    INTO v_unreturned_count
    FROM BorrowRecord
    WHERE ReaderID = p_reader_id
      AND Status IN ('借阅中', '逾期');

    IF v_unreturned_count >= 5 THEN
        RAISE_APPLICATION_ERROR(-20003, '当前未归还图书已达上限');
    END IF;

    SELECT Status
    INTO v_book_status
    FROM Book
    WHERE BookID = p_book_id
    FOR UPDATE;

    IF v_book_status <> '正常' THEN
        RAISE_APPLICATION_ERROR(-20004, '该图书当前不可借');
    END IF;

    INSERT INTO BorrowRecord (
        ReaderID,
        BookID,
        BorrowTime,
        DueTime,
        Status
    ) VALUES (
        p_reader_id,
        p_book_id,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP + INTERVAL '30' DAY,
        '借阅中'
    );

    UPDATE Book
    SET Status = '借出'
    WHERE BookID = p_book_id;

    COMMIT;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        ROLLBACK;
        RAISE_APPLICATION_ERROR(-20005, '读者或图书不存在');
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END;
/


/* =========================================================
   2. return_book
   功能：读者还书。
   业务规则：
   - 只能归还“借阅中”或“逾期”的记录
   - 自动计算逾期罚款
   - 图书状态恢复为“正常”
   - 逾期还书扣减读者信用分
   ========================================================= */
CREATE OR REPLACE PROCEDURE return_book (
    p_borrow_record_id IN INT
)
IS
    v_book_id INT;
    v_reader_id INT;
    v_due_time TIMESTAMP;
    v_fine DECIMAL(8, 2);
BEGIN
    SELECT BookID, ReaderID, DueTime
    INTO v_book_id, v_reader_id, v_due_time
    FROM BorrowRecord
    WHERE BorrowRecordID = p_borrow_record_id
      AND Status IN ('借阅中', '逾期')
    FOR UPDATE;

    v_fine := calculate_overdue_fine(v_due_time, CURRENT_TIMESTAMP);

    UPDATE BorrowRecord
    SET ReturnTime = CURRENT_TIMESTAMP,
        OverdueFine = v_fine,
        Status = '已归还'
    WHERE BorrowRecordID = p_borrow_record_id;

    UPDATE Book
    SET Status = '正常'
    WHERE BookID = v_book_id;

    IF v_fine > 0 THEN
        UPDATE Reader
        SET CreditScore = GREATEST(CreditScore - 5, 0)
        WHERE ReaderID = v_reader_id;
    END IF;

    COMMIT;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        ROLLBACK;
        RAISE_APPLICATION_ERROR(-20006, '有效借阅记录不存在或该记录已归还');
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END;
/


/* =========================================================
   3. process_overdue_records
   功能：处理逾期借阅记录。
   业务规则：
   - 将超过应还时间且仍为“借阅中”的记录改为“逾期”
   - 计算当前逾期罚款
   - 存在逾期记录的读者扣减信用分
   - 信用分低于 40 的读者自动冻结
   ========================================================= */
CREATE OR REPLACE PROCEDURE process_overdue_records
IS
BEGIN
    UPDATE BorrowRecord
    SET Status = '逾期',
        OverdueFine = calculate_overdue_fine(DueTime, CURRENT_TIMESTAMP)
    WHERE Status = '借阅中'
      AND DueTime < CURRENT_TIMESTAMP;

    UPDATE Reader r
    SET CreditScore = GREATEST(CreditScore - 10, 0)
    WHERE EXISTS (
        SELECT 1
        FROM BorrowRecord br
        WHERE br.ReaderID = r.ReaderID
          AND br.Status = '逾期'
    );

    UPDATE Reader
    SET AccountStatus = '冻结'
    WHERE CreditScore < 40;

    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END;
/


/* =========================================================
   4. handle_report
   功能：管理员处理评论举报。
   业务规则：
   - 只有“待处理”的举报可以处理
   - 处理动作只能为“驳回”或“处理完成”
   - 处理完成时屏蔽对应评论
   ========================================================= */
CREATE OR REPLACE PROCEDURE handle_report (
    p_report_id IN INT,
    p_librarian_id IN INT,
    p_action IN VARCHAR2,
    p_result IN CLOB
)
IS
    v_comment_id INT;
BEGIN
    IF p_action NOT IN ('驳回', '处理完成') THEN
        RAISE_APPLICATION_ERROR(-20010, '举报处理动作不合法');
    END IF;

    SELECT CommentID
    INTO v_comment_id
    FROM Report
    WHERE ReportID = p_report_id
      AND Status = '待处理'
    FOR UPDATE;

    UPDATE Report
    SET Status = p_action,
        LibrarianID = p_librarian_id,
        HandleTime = CURRENT_TIMESTAMP,
        HandleResult = p_result
    WHERE ReportID = p_report_id;

    IF p_action = '处理完成' THEN
        UPDATE Comment_Table
        SET Status = '屏蔽'
        WHERE CommentID = v_comment_id;
    END IF;

    COMMIT;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        ROLLBACK;
        RAISE_APPLICATION_ERROR(-20011, '待处理举报不存在');
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END;
/
