/* =========================================================
   三人版图书馆管理系统 - 函数脚本
   Database: Oracle 19c
   Depends on: schema_three_person.sql
   ========================================================= */


/* =========================================================
   1. calculate_overdue_fine
   功能：根据应还时间和实际归还/当前时间计算逾期罚款。
   规则：每逾期 1 天罚款 0.50 元，未逾期返回 0。
   ========================================================= */
CREATE OR REPLACE FUNCTION calculate_overdue_fine (
    p_due_time IN TIMESTAMP,
    p_check_time IN TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) RETURN DECIMAL
IS
    v_overdue_days NUMBER;
    v_fine DECIMAL(8, 2);
BEGIN
    IF p_due_time IS NULL OR p_check_time IS NULL THEN
        RETURN 0;
    END IF;

    IF p_check_time <= p_due_time THEN
        RETURN 0;
    END IF;

    v_overdue_days := TRUNC(CAST(p_check_time AS DATE)) - TRUNC(CAST(p_due_time AS DATE));

    IF v_overdue_days < 1 THEN
        v_overdue_days := 1;
    END IF;

    v_fine := v_overdue_days * 0.50;

    RETURN v_fine;
END;
/
