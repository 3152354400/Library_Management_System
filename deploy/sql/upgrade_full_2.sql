SET SQLBLANKLINES ON
SET DEFINE OFF
-- ================================================================
-- 完整版升级 SQL（第二部分）：视图 + 函数 + 过程 + 触发器
-- 依赖第一部分已执行
-- ================================================================

-- ---------- 1. 视图 ----------

-- 座位状态视图（匹配 SeatDto：CurrentStatus 列）
CREATE OR REPLACE VIEW V_Seat_Status AS
SELECT s.SeatID, s.BuildingID, s.Floor, s.SeatNumber, s.Zone,
       CASE WHEN EXISTS (
           SELECT 1 FROM Reserve_Seat rs
           WHERE rs.SeatID = s.SeatID AND rs.Status = '未完成'
             AND SYSTIMESTAMP BETWEEN rs.StartTime AND rs.EndTime
       ) THEN '已预约' ELSE '空闲' END AS CurrentStatus
FROM Seat s;

-- 我的预约视图（匹配 MyReservationDto）
CREATE OR REPLACE VIEW V_MyReservations AS
SELECT rs.ReservationID, rs.ReaderID, rs.SeatID,
       b.BuildingName, s.Floor, s.SeatNumber,
       rs.StartTime, rs.EndTime, rs.Status
FROM Reserve_Seat rs
JOIN Seat s ON rs.SeatID = s.SeatID
JOIN Building b ON s.BuildingID = b.BuildingID;

-- 采购分析：图书种类借阅 Top10（按 ISBN 借阅次数）
CREATE OR REPLACE VIEW V_BookRank_By_BorrowCount AS
SELECT bi.ISBN, bi.Title, bi.Author, COUNT(*) AS MetricValue
FROM BorrowRecord br
JOIN Book b ON br.BookID = b.BookID
JOIN BookInfo bi ON b.ISBN = bi.ISBN
GROUP BY bi.ISBN, bi.Title, bi.Author;

-- 采购分析：借阅总时长 Top10（按 ISBN 累计天数）
CREATE OR REPLACE VIEW V_BookRank_By_BorrowDuration AS
SELECT bi.ISBN, bi.Title, bi.Author,
       ROUND(SUM(NVL(EXTRACT(DAY FROM (NVL(br.ReturnTime, SYSTIMESTAMP) - br.BorrowTime)), 0))) AS MetricValue
FROM BorrowRecord br
JOIN Book b ON br.BookID = b.BookID
JOIN BookInfo bi ON b.ISBN = bi.ISBN
GROUP BY bi.ISBN, bi.Title, bi.Author;

-- 采购分析：单本图书借阅 Top10（按 Barcode 借阅次数）
CREATE OR REPLACE VIEW V_BookRank_By_InstanceBorrow AS
SELECT b.Barcode, bi.Title, bi.Author, COUNT(*) AS MetricValue
FROM BorrowRecord br
JOIN Book b ON br.BookID = b.BookID
JOIN BookInfo bi ON b.ISBN = bi.ISBN
GROUP BY b.Barcode, bi.Title, bi.Author;

-- 荐购推荐依赖视图（书单被收藏数 → 图书热度）
CREATE OR REPLACE VIEW book_list_rank_view AS
SELECT cl.ReaderID, bb.ISBN, COUNT(*) AS BooklistCount
FROM Collect cl
JOIN Booklist_Book bb ON cl.BooklistID = bb.BooklistID
GROUP BY cl.ReaderID, bb.ISBN;

-- ---------- 2. 函数 ----------

-- 图书推荐函数（荐购服务调用）
CREATE OR REPLACE FUNCTION get_recommendations(p_ReaderID INT, p_TopN INT)
RETURN SYS_REFCURSOR
AS
    v_cursor SYS_REFCURSOR;
BEGIN
    OPEN v_cursor FOR
    SELECT r.ISBN, bi.Title, bi.Author, r.BooklistCount
    FROM book_list_rank_view r
             JOIN BookInfo bi ON r.ISBN = bi.ISBN
    WHERE r.ReaderID = p_ReaderID
    ORDER BY r.BooklistCount DESC
    FETCH FIRST p_TopN ROWS ONLY;
    RETURN v_cursor;
END;
/

-- ---------- 3. 存储过程 ----------

-- 续借图书
CREATE OR REPLACE PROCEDURE sp_renew_book (
    p_reader_id IN INT,
    p_book_id IN VARCHAR2,
    p_result OUT VARCHAR2,
    p_new_due_time OUT TIMESTAMP
)
IS
    v_br_id INT;
    v_due TIMESTAMP;
    v_count INT;
BEGIN
    -- 优先按 BookID（数字）查找
    BEGIN
        SELECT BorrowRecordID, DueTime INTO v_br_id, v_due
        FROM BorrowRecord
        WHERE ReaderID = p_reader_id
          AND ReturnTime IS NULL
          AND Status IN ('借阅中', '逾期')
          AND BookID = TO_NUMBER(p_book_id)
        FETCH FIRST 1 ROWS ONLY;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            v_br_id := NULL;
        WHEN OTHERS THEN
            v_br_id := NULL; -- 非数字入参，走条码分支
    END;

    -- 未找到则按 Barcode 查找
    IF v_br_id IS NULL THEN
        BEGIN
            SELECT br.BorrowRecordID, br.DueTime INTO v_br_id, v_due
            FROM BorrowRecord br
            JOIN Book b ON br.BookID = b.BookID
            WHERE br.ReaderID = p_reader_id
              AND br.ReturnTime IS NULL
              AND br.Status IN ('借阅中', '逾期')
              AND b.Barcode = p_book_id
            FETCH FIRST 1 ROWS ONLY;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                p_result := 'ERROR: 未找到可续借的借阅记录';
                RETURN;
        END;
    END IF;

    SELECT COUNT(*) INTO v_count FROM RenewRecord WHERE BorrowRecordID = v_br_id;
    IF v_count >= 1 THEN
        p_result := 'ERROR: 该图书已续借过一次，不能再次续借';
        RETURN;
    END IF;

    v_due := v_due + INTERVAL '30' DAY;

    INSERT INTO RenewRecord (RenewID, BorrowRecordID, ReaderID, RenewTime, OldDueTime, NewDueTime, RenewCount, Status)
    VALUES (seq_renew_id.NEXTVAL, v_br_id, p_reader_id, SYSTIMESTAMP, v_due - INTERVAL '30' DAY, v_due, 1, '有效');

    UPDATE BorrowRecord SET DueTime = v_due WHERE BorrowRecordID = v_br_id;

    p_result := 'SUCCESS: 续借成功，新的应还时间为' || TO_CHAR(v_due, 'YYYY-MM-DD HH24:MI');
    p_new_due_time := v_due;
EXCEPTION
    WHEN OTHERS THEN
        p_result := 'ERROR: ' || SQLERRM;
END;
/

-- 预约图书
CREATE OR REPLACE PROCEDURE sp_reserve_book (
    p_reader_id IN INT,
    p_isbn IN VARCHAR2,
    p_expected_days IN INT,
    p_result OUT VARCHAR2,
    p_reserve_id OUT INT
)
IS
    v_count INT;
    v_reserve_id INT;
    v_expire TIMESTAMP;
BEGIN
    SELECT COUNT(*) INTO v_count FROM BookInfo WHERE ISBN = p_isbn;
    IF v_count = 0 THEN
        p_result := 'ERROR: 图书不存在';
        RETURN;
    END IF;

    SELECT COUNT(*) INTO v_count FROM ReserveBook
    WHERE ReaderID = p_reader_id AND ISBN = p_isbn AND Status IN ('等待中', '已通知');
    IF v_count > 0 THEN
        p_result := 'ERROR: 您已预约过该图书，请勿重复预约';
        RETURN;
    END IF;

    v_reserve_id := seq_reserve_id.NEXTVAL;
    v_expire := SYSTIMESTAMP + NVL(p_expected_days, 7);

    INSERT INTO ReserveBook (ReserveID, ReaderID, ISBN, ReserveTime, ExpectedBorrowTime, ExpireTime, Status, Priority)
    VALUES (v_reserve_id, p_reader_id, p_isbn, SYSTIMESTAMP, v_expire, v_expire + INTERVAL '3' DAY, '等待中', 0);

    p_result := 'SUCCESS: 预约成功';
    p_reserve_id := v_reserve_id;
EXCEPTION
    WHEN OTHERS THEN
        p_result := 'ERROR: ' || SQLERRM;
END;
/

-- 取消预约
CREATE OR REPLACE PROCEDURE sp_cancel_reserve (
    p_reserve_id IN INT,
    p_reader_id IN INT,
    p_cancel_reason IN VARCHAR2,
    p_result OUT VARCHAR2
)
IS
    v_rows INT;
BEGIN
    UPDATE ReserveBook
    SET Status = '已取消', CancelReason = p_cancel_reason, HandleTime = SYSTIMESTAMP
    WHERE ReserveID = p_reserve_id AND ReaderID = p_reader_id AND Status = '等待中';

    v_rows := SQL%ROWCOUNT;
    IF v_rows = 0 THEN
        p_result := 'ERROR: 预约不存在或当前状态无法取消';
    ELSE
        p_result := 'SUCCESS: 预约已取消';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        p_result := 'ERROR: ' || SQLERRM;
END;
/

-- 荐购图书
CREATE OR REPLACE PROCEDURE sp_recommend_purchase (
    p_reader_id IN INT,
    p_isbn IN VARCHAR2,
    p_title IN VARCHAR2,
    p_author IN VARCHAR2,
    p_publisher IN VARCHAR2,
    p_publish_year IN INT,
    p_reason IN VARCHAR2,
    p_result OUT VARCHAR2,
    p_recommend_id OUT INT
)
IS
    v_id INT;
BEGIN
    v_id := seq_recommend_id.NEXTVAL;

    INSERT INTO RecommendPurchase (RecommendID, ReaderID, ISBN, Title, Author, Publisher, PublishYear, Reason, RecommendTime, Status)
    VALUES (v_id, p_reader_id, p_isbn, p_title, p_author, p_publisher, p_publish_year, p_reason, SYSTIMESTAMP, '待审核');

    p_result := 'SUCCESS: 荐购提交成功，等待管理员审核';
    p_recommend_id := v_id;
EXCEPTION
    WHEN OTHERS THEN
        p_result := 'ERROR: ' || SQLERRM;
END;
/

-- 管理员处理荐购
CREATE OR REPLACE PROCEDURE sp_handle_recommend (
    p_recommend_id IN INT,
    p_handler_id IN INT,
    p_action IN VARCHAR2,
    p_handle_result IN VARCHAR2,
    p_purchase_price IN DECIMAL,
    p_result OUT VARCHAR2
)
IS
    v_status VARCHAR2(20);
    v_rows INT;
BEGIN
    IF p_action IN ('采纳', 'approve', 'APPROVE') THEN
        v_status := '已采纳';
    ELSIF p_action IN ('购买', 'purchase', 'PURCHASE') THEN
        v_status := '已购买';
    ELSIF p_action IN ('拒绝', 'reject', 'REJECT') THEN
        v_status := '被拒绝';
    ELSE
        p_result := 'ERROR: 处理动作不合法';
        RETURN;
    END IF;

    UPDATE RecommendPurchase
    SET Status = v_status,
        HandlerID = p_handler_id,
        HandleTime = SYSTIMESTAMP,
        HandleResult = p_handle_result,
        PurchasePrice = NVL(p_purchase_price, PurchasePrice),
        PurchaseTime = CASE WHEN v_status = '已购买' THEN SYSTIMESTAMP ELSE PurchaseTime END
    WHERE RecommendID = p_recommend_id;

    v_rows := SQL%ROWCOUNT;
    IF v_rows = 0 THEN
        p_result := 'ERROR: 荐购记录不存在';
    ELSE
        p_result := 'SUCCESS: 处理成功';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        p_result := 'ERROR: ' || SQLERRM;
END;
/

-- 缴纳罚款
CREATE OR REPLACE PROCEDURE sp_pay_fine (
    p_fine_id IN INT,
    p_pay_amount IN DECIMAL,
    p_pay_method IN VARCHAR2,
    p_operator_id IN INT,
    p_result OUT VARCHAR2
)
IS
    v_amount DECIMAL(8,2);
    v_paid DECIMAL(8,2);
    v_new_paid DECIMAL(8,2);
BEGIN
    SELECT Amount, PaidAmount INTO v_amount, v_paid
    FROM Fine WHERE FineID = p_fine_id FOR UPDATE;

    IF v_paid >= v_amount THEN
        p_result := 'ERROR: 该罚款已缴清';
        RETURN;
    END IF;

    v_new_paid := v_paid + NVL(p_pay_amount, 0);

    IF v_new_paid > v_amount THEN
        p_result := 'ERROR: 缴纳金额超过应缴金额，当前待缴' || TO_CHAR(v_amount - v_paid);
        RETURN;
    END IF;

    UPDATE Fine
    SET PaidAmount = v_new_paid,
        PayStatus = CASE WHEN v_new_paid >= v_amount THEN '已缴纳' ELSE '部分缴纳' END,
        PayTime = SYSTIMESTAMP,
        HandlerID = p_operator_id
    WHERE FineID = p_fine_id;

    INSERT INTO FinePayment (PaymentID, FineID, PayAmount, PayTime, PayMethod, OperatorID)
    VALUES (seq_payment_id.NEXTVAL, p_fine_id, p_pay_amount, SYSTIMESTAMP, p_pay_method, p_operator_id);

    p_result := 'SUCCESS: 缴纳成功';
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_result := 'ERROR: 罚款记录不存在';
    WHEN OTHERS THEN
        p_result := 'ERROR: ' || SQLERRM;
END;
/

-- ---------- 4. 触发器 ----------

-- 还书时自动生成罚款、通知、操作日志
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
        SYSTIMESTAMP + 30,
        '未缴纳',
        '系统自动生成的逾期罚款'
    )
    RETURNING FineID INTO v_fine_id;

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

-- 预约过期自动置为已过期
CREATE OR REPLACE TRIGGER trg_expire_reservation
BEFORE UPDATE ON ReserveBook
FOR EACH ROW
WHEN (OLD.Status = '等待中' AND OLD.ExpireTime < SYSTIMESTAMP)
BEGIN
    :NEW.Status := '已过期';
END;
/

-- 通知过期自动置为已过期
CREATE OR REPLACE TRIGGER trg_expire_notification
BEFORE UPDATE ON Notification
FOR EACH ROW
WHEN (OLD.ExpireTime IS NOT NULL AND OLD.ExpireTime < SYSTIMESTAMP AND OLD.Status = '有效')
BEGIN
    :NEW.Status := '已过期';
END;
/

-- 评论删除时自动驳回相关举报
CREATE OR REPLACE TRIGGER trg_cascade_comment_delete
AFTER UPDATE ON Comment_Table
FOR EACH ROW
WHEN (OLD.Status = '正常' AND NEW.Status = '已删除')
BEGIN
    UPDATE Report
    SET Status = '驳回',
        HandleResult = CONCAT(NVL(HandleResult, ''), ' | 评论已删除，举报自动驳回')
    WHERE CommentID = :NEW.CommentID
      AND Status = '待处理';
END;
/

COMMIT;
