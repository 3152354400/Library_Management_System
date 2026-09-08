/* ---------- 5. Building ---------- */
INSERT INTO Building (BuildingName, Address, TotalFloors, OpenHours, LibrarianID, Remark)
VALUES ('总图书馆',
        '上海市杨浦区四平路1239号',
        7,
        '08:00-22:30',
        8,  -- 负责人 ID，对应 LibrarianID=8
        '主馆');

INSERT INTO Building (BuildingName, Address, TotalFloors, OpenHours, LibrarianID, Remark)
VALUES ('德文图书馆',
        '上海市杨浦区四平路1239号',
        2,
        '09:00-18:00',
        8,  -- 负责人 ID，对应 LibrarianID=8
        '外文馆');