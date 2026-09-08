-- 处理所有逾期记录：更新罚款、扣信用分、冻结账户
CREATE OR REPLACE PROCEDURE ProcessOverdueRecords AS
    -- 游标：查询所有未归还且已逾期的记录
    CURSOR c_overdue IS
        SELECT 
            br.BorrowRecordID, 
            br.ReaderID, 
            br.BorrowTime, 
        FROM BorrowRecord br
        WHERE br.ReturnTime IS NULL  -- 未归还
          AND br.BorrowTime + 0 < TRUNC(SYSDATE);  -- 已过应还时间

    v_fine NUMBER(10,2);  -- 计算的罚款
    --v_credit_deduct INT;  -- 信用分扣减额
    v_days_overdue INT;  -- 逾期天数
BEGIN
    FOR rec IN c_overdue LOOP
        -- 1. 计算逾期天数和罚款
        v_days_overdue := TRUNC(SYSDATE) - TRUNC(rec.BorrowTime+0);
        v_fine := CalculateOverdueFine(rec.DueTime, NULL);  -- 未还，第二个参数为NULL

        -- 2. 更新借阅记录的罚款
        UPDATE BorrowRecord
        SET OverdueFine = v_fine
        WHERE BorrowRecordID = rec.BorrowRecordID;

        -- -- 3. 计算信用分扣减（首次5分，后续10分）
        -- IF rec.IsFirstOverdue = 1 THEN
        --     v_credit_deduct := 5;
        --     -- 标记为非首次逾期（仅首次处理时更新）
        --     UPDATE BorrowRecord
        --     SET IsFirstOverdue = 0
        --     WHERE BorrowRecordID = rec.BorrowRecordID;
        -- ELSE
        --     v_credit_deduct := 10;
        -- END IF;

        -- -- 4. 更新读者信用分和逾期次数
        -- UPDATE Reader
        -- SET CreditScore = GREATEST(rec.CreditScore - v_credit_deduct, 0),  -- 最低0分
        --     OverdueCount = rec.OverdueCount + 1
        -- WHERE ReaderID = rec.ReaderID;

        -- -- 5. 若信用分<60，冻结账户
        -- UPDATE Reader
        -- SET AccountStatus = '冻结'
        -- WHERE ReaderID = rec.ReaderID
        --   AND CreditScore < 60;
    END LOOP;

    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;  -- 抛出异常，便于调试
END ProcessOverdueRecords;
/