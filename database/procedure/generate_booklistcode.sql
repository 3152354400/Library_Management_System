-- 生成唯一的书单代码
CREATE OR REPLACE FUNCTION GenerateListCode RETURN VARCHAR2
IS
    v_Code VARCHAR2(20);
    v_Count NUMBER;
BEGIN
    LOOP
        --生成随机字母数字代码
        v_Code := DBMS_RANDOM.STRING('X', 8);
        --检查是否已经存在
        SELECT COUNT(*) INTO v_Count FROM Booklist WHERE ListCode = v_Code;
        EXIT WHEN v_Count = 0;
    END LOOP;
    RETURN v_Code;
END;
/