-- ================================================================
-- 缴纳罚款存储过程
-- 支持读者缴纳罚款
-- ================================================================
CREATE OR REPLACE PROCEDURE sp_pay_fine(
    p_fine_id IN INT,
    p_pay_amount IN DECIMAL,
    p_pay_method IN VARCHAR2,
    p_operator_id IN INT,
    p_result OUT VARCHAR2
) AS
    v_fine_amount DECIMAL(8,2);
    v_paid_amount DECIMAL(8,2);
    v_remaining DECIMAL(8,2);
    v_reader_id INT;
    v_payment_id INT;
BEGIN
    p_result := '';

    -- 1. 查询罚款信息
    SELECT Amount, PaidAmount, ReaderID
    INTO v_fine_amount, v_paid_amount, v_reader_id
    FROM Fine
    WHERE FineID = p_fine_id;

    -- 2. 校验支付状态
    IF v_paid_amount >= v_fine_amount THEN
        p_result := 'ERROR: 该罚款已缴清，无需再次缴纳';
        RETURN;
    END IF;

    -- 3. 计算待缴金额
    v_remaining := v_fine_amount - v_paid_amount;

    IF p_pay_amount <= 0 THEN
        p_result := 'ERROR: 缴纳金额必须大于0';
        RETURN;
    END IF;

    IF p_pay_amount > v_remaining THEN
        p_result := 'ERROR: 缴纳金额超出待缴金额（待缴：' || v_remaining || '元）';
        RETURN;
    END IF;

    -- 4. 记录缴纳明细
    INSERT INTO FinePayment (PaymentID, FineID, PayAmount, PayTime, PayMethod, OperatorID)
    VALUES (seq_payment_id.NEXTVAL, p_fine_id, p_pay_amount, SYSTIMESTAMP, p_pay_method, p_operator_id)
    RETURNING PaymentID INTO v_payment_id;

    -- 5. 更新罚款表的已缴金额
    UPDATE Fine
    SET PaidAmount = PaidAmount + p_pay_amount,
        HandlerID = p_operator_id
    WHERE FineID = p_fine_id;

    -- 6. 检查是否缴清
    IF v_paid_amount + p_pay_amount >= v_fine_amount THEN
        UPDATE Fine
        SET PayStatus = '已缴纳',
            PayTime = SYSTIMESTAMP
        WHERE FineID = p_fine_id;
    ELSE
        UPDATE Fine
        SET PayStatus = '部分缴纳'
        WHERE FineID = p_fine_id;
    END IF;

    -- 7. 发送缴纳成功通知
    INSERT INTO Notification (
        NotificationID, ReaderID, Title, Content, Type,
        RelatedType, RelatedID, SenderType, CreateTime
    ) VALUES (
        seq_notif_id.NEXTVAL, v_reader_id,
        '罚款缴纳成功',
        '您已成功缴纳罚款' || p_pay_amount || '元，还需缴纳' || (v_fine_amount - v_paid_amount - p_pay_amount) || '元。',
        '罚款通知',
        'Fine', p_fine_id,
        '系统', SYSTIMESTAMP
    );

    -- 8. 记录操作日志
    INSERT INTO OperationLog (
        LogID, OperationTime, OperatorID, OperatorType,
        OperationModule, OperationAction, TargetType, TargetID,
        OperationDetail, Status
    ) VALUES (
        seq_log_id.NEXTVAL, SYSTIMESTAMP, TO_CHAR(p_operator_id), 'Librarian',
        '罚款管理', '缴纳罚款', 'Fine', TO_CHAR(p_fine_id),
        '管理员' || p_operator_id || '为读者' || v_reader_id || '办理罚款缴纳，金额：' || p_pay_amount || '元',
        '成功'
    );

    p_result := 'SUCCESS: 缴纳成功！缴纳金额：' || p_pay_amount || '元';
    COMMIT;

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_result := 'ERROR: 罚款记录不存在';
        ROLLBACK;
    WHEN OTHERS THEN
        p_result := 'ERROR: ' || SQLERRM;
        ROLLBACK;
END sp_pay_fine;
/
