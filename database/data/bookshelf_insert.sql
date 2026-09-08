-- SET SERVEROUTPUT ON
DECLARE
    /* 公共过程，入参控制楼层/区/书柜数 */
    PROCEDURE add_shelves (
        p_building_id  IN NUMBER,  -- 改为NUMBER类型
        p_max_floor    IN PLS_INTEGER,
        p_zone_count   IN PLS_INTEGER,
        p_shelf_each   IN PLS_INTEGER )
    IS
    BEGIN
        FOR f IN 1 .. p_max_floor LOOP            -- 楼层
            FOR z IN 1 .. p_zone_count LOOP       -- 区域 A/B/...
                FOR s IN 1 .. p_shelf_each LOOP   -- 同区内书架序号
                    INSERT INTO Bookshelf
                          (BuildingID,
                           ShelfCode,
                           Floor,
                           Zone)
                    VALUES
                          (p_building_id,  -- 使用数字ID
                           LPAD(f, 2, '0') || CHR(64+z) || '-' || LPAD(s, 3, '0'),
                           f,
                           CHR(64+z) || '区');
                END LOOP;
            END LOOP;
        END LOOP;
        DBMS_OUTPUT.PUT_LINE(
          p_building_id || ': 完成 ' ||
          p_max_floor*p_zone_count*p_shelf_each || ' 条记录');
    END add_shelves;
BEGIN
    /* 总图书馆：14 层，每层 4 区 (A~D)，每区 10 个书架 */
    add_shelves(21, 14, 4, 10);  -- 使用数字21

    /* 德文图书馆：2 层，每层 2 区 (A~B)，每区 5 个书架 */
    add_shelves(22, 2, 2, 5);    -- 使用数字22

END;
/