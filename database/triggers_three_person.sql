/* =========================================================
   三人版图书馆管理系统 - 触发器脚本
   Database: Oracle 19c
   Depends on: schema_three_person.sql
   ========================================================= */


/* =========================================================
   1. trg_book_after_insert
   功能：新增实体馆藏图书后，同步 BookInfo 的总库存和可借库存。
   ========================================================= */
CREATE OR REPLACE TRIGGER trg_book_after_insert
AFTER INSERT ON Book
FOR EACH ROW
BEGIN
    UPDATE BookInfo
    SET TotalStock = TotalStock + 1,
        AvailableStock = CASE
            WHEN :NEW.Status = '正常' THEN AvailableStock + 1
            ELSE AvailableStock
        END
    WHERE ISBN = :NEW.ISBN;
END;
/


/* =========================================================
   2. trg_book_status_update
   功能：实体馆藏图书状态变化后，同步 BookInfo 的可借库存。
   说明：
   - 正常 -> 借出/下架/遗失：可借库存减少
   - 借出/下架/遗失 -> 正常：可借库存增加
   ========================================================= */
CREATE OR REPLACE TRIGGER trg_book_status_update
AFTER UPDATE OF Status ON Book
FOR EACH ROW
BEGIN
    IF :OLD.Status = '正常' AND :NEW.Status IN ('借出', '下架', '遗失') THEN
        UPDATE BookInfo
        SET AvailableStock = AvailableStock - 1
        WHERE ISBN = :NEW.ISBN;
    ELSIF :OLD.Status IN ('借出', '下架', '遗失') AND :NEW.Status = '正常' THEN
        UPDATE BookInfo
        SET AvailableStock = AvailableStock + 1
        WHERE ISBN = :NEW.ISBN;
    END IF;
END;
/
