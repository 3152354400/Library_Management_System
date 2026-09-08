-- 计算逾期罚款：超过应还时间的天数 × 0.5元/天
CREATE OR REPLACE FUNCTION CalculateOverdueFine(
    p_borrow_time DATE,  -- 传入借阅时间
    p_return_time DATE   -- 实际归还时间（NULL表示未还）
) RETURN NUMBER AS
    v_due_time DATE;     -- 应还时间
    v_days_overdue INT;  -- 逾期天数
    v_fine NUMBER(10,2); -- 罚款金额
BEGIN
    -- 计算应还时间：借阅时间 + 3天（此处可修改为任意固定天数）
    v_due_time := p_borrow_time + 0;
    
    -- 逾期判定：未归还 或 实际归还时间 > 应还时间
    IF p_return_time IS NULL OR p_return_time > v_due_time THEN
        -- 计算逾期天数：当前时间 - 应还时间（截断除时分秒，按天计算）
        v_days_overdue := TRUNC(SYSDATE) - TRUNC(v_due_time);
        -- 罚款 = 逾期天数 × 0.5元/天（若未逾期则为0）
        v_fine := GREATEST(v_days_overdue, 0) * 0.5;
    ELSE
        v_fine := 0;  -- 未逾期，罚款为0
    END IF;
    
    RETURN v_fine;
END CalculateOverdueFine;
/