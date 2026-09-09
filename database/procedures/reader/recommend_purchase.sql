-- ================================================================
-- 图书荐购存储过程
-- 支持读者推荐图书馆购买新书
-- ================================================================
CREATE OR REPLACE PROCEDURE sp_recommend_purchase(
    p_reader_id IN INT,
    p_isbn IN VARCHAR2,
    p_title IN VARCHAR2,
    p_author IN VARCHAR2,
    p_publisher IN VARCHAR2,
    p_publish_year IN INT,
    p_reason IN VARCHAR2,
    p_result OUT VARCHAR2,
    p_recommend_id OUT INT
) AS
    v_exists_isbn INT;
    v_exists_recommend INT;
    v_max_recommend_per_month INT := 5;
    v_current_month_count INT;
BEGIN
    p_result := '';
    p_recommend_id := 0;

    -- 1. 校验必填项
    IF p_title IS NULL OR LENGTH(TRIM(p_title)) = 0 THEN
        p_result := 'ERROR: 书名不能为空';
        RETURN;
    END IF;

    -- 2. 检查是否已存在该ISBN的图书
    IF p_isbn IS NOT NULL AND LENGTH(TRIM(p_isbn)) > 0 THEN
        SELECT COUNT(*) INTO v_exists_isbn
        FROM BookInfo
        WHERE ISBN = p_isbn;

        IF v_exists_isbn > 0 THEN
            p_result := 'ERROR: 该ISBN对应的图书已在图书馆馆藏中，无需荐购';
            RETURN;
        END IF;
    END IF;

    -- 3. 检查是否重复荐购
    SELECT COUNT(*)
    INTO v_exists_recommend
    FROM RecommendPurchase
    WHERE ReaderID = p_reader_id
      AND Title = p_title
      AND Status IN ('待审核', '已采纳');

    IF v_exists_recommend > 0 THEN
        p_result := 'ERROR: 您已荐购过该图书，请勿重复提交';
        RETURN;
    END IF;

    -- 4. 检查月度荐购数量限制
    SELECT COUNT(*)
    INTO v_current_month_count
    FROM RecommendPurchase
    WHERE ReaderID = p_reader_id
      AND TO_CHAR(RecommendTime, 'YYYY-MM') = TO_CHAR(SYSDATE, 'YYYY-MM');

    IF v_current_month_count >= v_max_recommend_per_month THEN
        p_result := 'ERROR: 本月已提交' || v_current_month_count || '次荐购，已达上限（' || v_max_recommend_per_month || '次）';
        RETURN;
    END IF;

    -- 5. 创建荐购记录
    INSERT INTO RecommendPurchase (
        ReaderID,
        ISBN,
        Title,
        Author,
        Publisher,
        PublishYear,
        Reason,
        RecommendTime,
        Status
    ) VALUES (
        p_reader_id,
        p_isbn,
        p_title,
        p_author,
        p_publisher,
        p_publish_year,
        p_reason,
        SYSTIMESTAMP,
        '待审核'
    )
    RETURNING RecommendID INTO p_recommend_id;

    -- 6. 发送提交成功通知
    INSERT INTO Notification (
        NotificationID, ReaderID, Title, Content, Type,
        RelatedType, RelatedID, SenderType, CreateTime
    ) VALUES (
        seq_notif_id.NEXTVAL, p_reader_id,
        '图书荐购提交成功',
        '您荐购的图书《' || p_title || '》已提交，等待管理员审核。',
        '其他',
        'RecommendPurchase', p_recommend_id,
        '系统', SYSTIMESTAMP
    );

    -- 7. 记录操作日志
    INSERT INTO OperationLog (
        LogID, OperationTime, OperatorID, OperatorType,
        OperationModule, OperationAction, TargetType, TargetID,
        OperationDetail, Status
    ) VALUES (
        seq_log_id.NEXTVAL, SYSTIMESTAMP, TO_CHAR(p_reader_id), 'Reader',
        '荐购管理', '提交荐购', 'RecommendPurchase', TO_CHAR(p_recommend_id),
        '读者' || p_reader_id || '提交图书荐购：《' || p_title || '》',
        '成功'
    );

    p_result := 'SUCCESS: 荐购提交成功！感谢您的荐购，我们会尽快处理';
    COMMIT;

EXCEPTION
    WHEN OTHERS THEN
        p_result := 'ERROR: ' || SQLERRM;
        ROLLBACK;
END sp_recommend_purchase;
/

-- ================================================================
-- 查询读者荐购列表
-- ================================================================
CREATE OR REPLACE PROCEDURE sp_get_reader_recommends(
    p_reader_id IN INT,
    p_result OUT SYS_REFCURSOR
) AS
BEGIN
    OPEN p_result FOR
    SELECT 
        rp.RecommendID,
        rp.ISBN,
        rp.Title,
        rp.Author,
        rp.Publisher,
        rp.PublishYear,
        rp.Reason,
        rp.RecommendTime,
        rp.Status,
        rp.HandleTime,
        rp.HandleResult
    FROM RecommendPurchase rp
    WHERE rp.ReaderID = p_reader_id
    ORDER BY rp.RecommendTime DESC;
END sp_get_reader_recommends;
/

-- ================================================================
-- 管理员处理荐购
-- ================================================================
CREATE OR REPLACE PROCEDURE sp_handle_recommend(
    p_recommend_id IN INT,
    p_handler_id IN INT,
    p_action IN VARCHAR2,
    p_handle_result IN VARCHAR2,
    p_purchase_price IN DECIMAL DEFAULT NULL,
    p_result OUT VARCHAR2
) AS
    v_status VARCHAR2(20);
    v_reader_id INT;
    v_title VARCHAR2(200);
BEGIN
    p_result := '';

    -- 1. 获取当前状态
    SELECT Status, ReaderID, Title INTO v_status, v_reader_id, v_title
    FROM RecommendPurchase
    WHERE RecommendID = p_recommend_id;

    IF v_status != '待审核' THEN
        p_result := 'ERROR: 该荐购记录已处理，无法重复操作';
        RETURN;
    END IF;

    -- 2. 执行处理
    IF p_action = '采纳' THEN
        UPDATE RecommendPurchase
        SET Status = '已采纳',
            HandlerID = p_handler_id,
            HandleTime = SYSTIMESTAMP,
            HandleResult = p_handle_result,
            PurchasePrice = p_purchase_price
        WHERE RecommendID = p_recommend_id;

        p_result := 'SUCCESS: 已采纳荐购';

    ELSIF p_action = '拒绝' THEN
        UPDATE RecommendPurchase
        SET Status = '被拒绝',
            HandlerID = p_handler_id,
            HandleTime = SYSTIMESTAMP,
            HandleResult = p_handle_result
        WHERE RecommendID = p_recommend_id;

        p_result := 'SUCCESS: 已拒绝荐购';

    ELSE
        p_result := 'ERROR: 无效的操作类型';
        RETURN;
    END IF;

    -- 3. 发送通知给读者
    INSERT INTO Notification (
        NotificationID, ReaderID, Title, Content, Type,
        RelatedType, RelatedID, SenderType, CreateTime
    ) VALUES (
        seq_notif_id.NEXTVAL, v_reader_id,
        '图书荐购处理结果',
        '您荐购的图书《' || v_title || '》已被管理员' || p_action || '，回复：' || NVL(p_handle_result, '无'),
        '其他',
        'RecommendPurchase', p_recommend_id,
        '系统', SYSTIMESTAMP
    );

    -- 4. 记录操作日志
    INSERT INTO OperationLog (
        LogID, OperationTime, OperatorID, OperatorType,
        OperationModule, OperationAction, TargetType, TargetID,
        OperationDetail, Status
    ) VALUES (
        seq_log_id.NEXTVAL, SYSTIMESTAMP, TO_CHAR(p_handler_id), 'Librarian',
        '荐购管理', '处理荐购', 'RecommendPurchase', TO_CHAR(p_recommend_id),
        '管理员' || p_handler_id || p_action || '读者荐购：《' || v_title || '》',
        '成功'
    );

    COMMIT;

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_result := 'ERROR: 荐购记录不存在';
        ROLLBACK;
    WHEN OTHERS THEN
        p_result := 'ERROR: ' || SQLERRM;
        ROLLBACK;
END sp_handle_recommend;
/
