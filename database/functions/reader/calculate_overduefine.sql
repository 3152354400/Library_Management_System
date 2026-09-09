-- ================================================================
-- 计算逾期罚款函数
-- 计算规则：超过应还时间的天数 × 0.5元/天
-- 借阅期限：30天
-- ================================================================
CREATE OR REPLACE FUNCTION CalculateOverdueFine(
    p_borrow_time IN DATE,  -- 传入借阅时间
    p_return_time IN DATE   -- 实际归还时间（NULL表示未还）
) RETURN NUMBER AS
    v_due_time DATE;        -- 应还时间
    v_days_overdue INT;     -- 逾期天数
    v_fine NUMBER(10,2);    -- 罚款金额
    v_borrow_days INT := 30; -- 借阅期限（天）
BEGIN
    -- 计算应还时间：借阅时间 + 借阅期限
    v_due_time := p_borrow_time + v_borrow_days;

    -- 逾期判定：未归还 或 实际归还时间 > 应还时间
    IF p_return_time IS NULL OR p_return_time > v_due_time THEN
        -- 计算逾期天数：当前时间 - 应还时间（截断时分秒，按天计算）
        v_days_overdue := TRUNC(SYSDATE) - TRUNC(v_due_time);
        -- 罚款 = 逾期天数 × 0.5元/天（若未逾期则为0）
        v_fine := GREATEST(v_days_overdue, 0) * 0.5;
    ELSE
        v_fine := 0;  -- 未逾期，罚款为0
    END IF;

    RETURN v_fine;
END CalculateOverdueFine;
/
