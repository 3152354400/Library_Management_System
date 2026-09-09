-- ================================================================
-- 扩展 BorrowRecord / Reader / Booklist 表（幂等版本）
-- 说明：
--  1. 本脚本用于"main 原 21 表 + 扩展"路线，走删减版 15 表路线时无需执行。
--  2. 已改为幂等：重复执行不会因"列已存在 / 约束重名"报错。
--  3. 修复原版缺失：BorrowRecord 补充 Status 列（process_overdue.sql 依赖它）。
--  4. Booklist 的 ListCode 若 schema_new.sql 已建则自动跳过，不再重复添加。
-- 执行方式：SQL Developer 直接运行（不再依赖 SQL*Plus @ 语法）。
-- ================================================================

DECLARE
    v_cnt NUMBER;
BEGIN
    -- ================================================================
    -- 1. 扩展 BorrowRecord：DueTime / IsFirstOverdue / OverdueCount / Status
    -- ================================================================
    SELECT COUNT(*) INTO v_cnt FROM USER_TAB_COLUMNS
    WHERE TABLE_NAME = 'BORROWRECORD' AND COLUMN_NAME = 'DUETIME';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE BorrowRecord ADD (DueTime TIMESTAMP, IsFirstOverdue INT DEFAULT 1, OverdueCount INT DEFAULT 0)';
        EXECUTE IMMEDIATE 'COMMENT ON COLUMN BorrowRecord.DueTime IS ''应还时间''';
        EXECUTE IMMEDIATE 'COMMENT ON COLUMN BorrowRecord.IsFirstOverdue IS ''是否首次逾期：1是，0否''';
        EXECUTE IMMEDIATE 'COMMENT ON COLUMN BorrowRecord.OverdueCount IS ''逾期次数''';
        -- 更新现有记录的 DueTime（BorrowTime + 30 天）
        EXECUTE IMMEDIATE 'UPDATE BorrowRecord SET DueTime = TO_TIMESTAMP(TO_CHAR(BorrowTime + 30, ''YYYY-MM-DD HH24:MI:SS''), ''YYYY-MM-DD HH24:MI:SS'') WHERE DueTime IS NULL';
    END IF;

    -- Status 列：process_overdue.sql 的游标与 WHERE 均引用它，必须存在
    SELECT COUNT(*) INTO v_cnt FROM USER_TAB_COLUMNS
    WHERE TABLE_NAME = 'BORROWRECORD' AND COLUMN_NAME = 'STATUS';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE BorrowRecord ADD (Status VARCHAR2(10) DEFAULT ''借阅中'' CHECK (Status IN (''借阅中'', ''已归还'', ''逾期'', ''遗失'')))';
        EXECUTE IMMEDIATE 'COMMENT ON COLUMN BorrowRecord.Status IS ''借阅状态：借阅中、已归还、逾期、遗失''';
        -- 历史记录：已还的置为"已归还"，未还的保持"借阅中"
        EXECUTE IMMEDIATE 'UPDATE BorrowRecord SET Status = ''已归还'' WHERE ReturnTime IS NOT NULL';
        EXECUTE IMMEDIATE 'UPDATE BorrowRecord SET Status = ''借阅中'' WHERE Status IS NULL';
    END IF;

    -- ================================================================
    -- 2. 扩展 Reader：OverdueCount
    -- ================================================================
    SELECT COUNT(*) INTO v_cnt FROM USER_TAB_COLUMNS
    WHERE TABLE_NAME = 'READER' AND COLUMN_NAME = 'OVERDUECOUNT';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE Reader ADD (OverdueCount INT DEFAULT 0)';
        EXECUTE IMMEDIATE 'COMMENT ON COLUMN Reader.OverdueCount IS ''逾期次数统计''';
    END IF;

    -- ================================================================
    -- 3. 扩展 Booklist：ListCode / CreateTime / Status
    --    （ListCode 若 schema_new.sql 已建且带唯一约束，则跳过，避免 ORA-01430/ORA-02264）
    -- ================================================================
    SELECT COUNT(*) INTO v_cnt FROM USER_TAB_COLUMNS
    WHERE TABLE_NAME = 'BOOKLIST' AND COLUMN_NAME = 'LISTCODE';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE Booklist ADD (ListCode VARCHAR2(20), CreateTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP, Status VARCHAR2(10) DEFAULT ''公开'' CHECK (Status IN (''公开'', ''私有'', ''删除'')))';
        EXECUTE IMMEDIATE 'COMMENT ON COLUMN Booklist.ListCode IS ''书单编码，唯一''';
        EXECUTE IMMEDIATE 'COMMENT ON COLUMN Booklist.CreateTime IS ''创建时间''';
        EXECUTE IMMEDIATE 'COMMENT ON COLUMN Booklist.Status IS ''书单状态：公开、私有、删除''';

        -- 为现有书单生成 ListCode
        FOR rec IN (SELECT BooklistID FROM Booklist WHERE ListCode IS NULL) LOOP
            EXECUTE IMMEDIATE 'UPDATE Booklist SET ListCode = ''BL'' || LPAD(' || rec.BooklistID || ', 10, ''0'') WHERE BooklistID = ' || rec.BooklistID;
        END LOOP;

        -- 添加唯一约束（先检查约束名是否已存在）
        SELECT COUNT(*) INTO v_cnt FROM USER_CONSTRAINTS
        WHERE CONSTRAINT_NAME IN ('UK_BOOKLIST_LISTCODE', 'UQ_BOOKLIST_CODE');
        IF v_cnt = 0 THEN
            EXECUTE IMMEDIATE 'ALTER TABLE Booklist ADD CONSTRAINT uk_booklist_listcode UNIQUE (ListCode)';
        END IF;
    END IF;

    COMMIT;
END;
/
