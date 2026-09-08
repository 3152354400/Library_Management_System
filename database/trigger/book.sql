-- 创建循环序列：
CREATE SEQUENCE seq_barcode
  START WITH 1
  INCREMENT BY 1
  MINVALUE 1
  MAXVALUE 99999
  CYCLE
  CACHE 20;

-- for TABLE Book ，attribute barcode
-- 创建触发器：自动填充 Barcode（BC + YYYYMMDD + 5位流水号）
CREATE OR REPLACE TRIGGER trg_book_barcode
BEFORE INSERT ON Book
FOR EACH ROW
BEGIN
  IF :NEW.Barcode IS NULL THEN
    :NEW.Barcode :='BC'
                    || TO_CHAR(SYSDATE, 'YYYYMMDD')       -- 日期前缀
                    || LPAD(seq_barcode.NEXTVAL, 5, '0'); -- 00001–99999
  END IF;
END;