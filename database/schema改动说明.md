## 1. 改动说明与收益

| 表                         | 旧主键 / 业务键                                     | 新主键 / 业务键                                 | 主要收益             |
| ------------------------- | --------------------------------------------- | ----------------------------------------- | ---------------- |
| **Reader**                | `ReaderID` VARCHAR → 主键                       | `ReaderID` INT IDENTITY；`Username` UNIQUE | 防穷举安全、索引小；学号变化后，无需级联更改  |
| **Librarian**             | `LibrarianID` VARCHAR     | `LibrarianID` INT；`StaffNo` UNIQUE        | 工号变更不影响外键；统一数值键  |
| **Building**              | `ManagerID` VARCHAR FK    | `LibrarianID` INT          | 加速 JOIN，避免类型不一致  |
| **Bookshelf**             | 复合 (BuildingID,ShelfCode) | `ShelfID` INT 主键；位置列 UNIQUE               | 外键只需 1 列；位置重排无需级联更改 |
| **Seat**                  | 复合主键                      | `SeatID` INT 主键；楼层/号 UNIQUE               | 同上，预约表少 2 列外键    |
| **StudyRoom**             | 复合主键                      | `RoomID` INT 主键；楼层/号 UNIQUE               | 同上               |
| **Book**                  | `BookID` VARCHAR         | `BookID` INT 主键；`Barcode` UNIQUE          | 条码换新不动外键；借阅表更快   |
| **Booklist**              | `BooklistID` VARCHAR      | `BooklistID` INT 主键；`ListCode` UNIQUE     | 外部系统只暴露 ListCode：FAV-2025-07作为路由，而不暴露真正的主键 BooklistPK，提升安全性。  |
| **BorrowRecord**          | `BookID` 外键              | `BookID` 外键                               | 数值 JOIN 提升性能     |
| **Announcement / Report** | `LibrarianID` VARCHAR 外键                      | `LibrarianID` INT 外键                      | 一致性、索引尺寸优化       |

## 2. 关键好处一览
安全性
数值主键不可预测，REST 路径不再泄露业务规律，防止 ID 枚举攻击。

查询与存储效率
INT 索引 < 字符串索引；复合键拆为单键后，外键表节省存储、加速 JOIN。

业务解耦
条码、工号、书架号等业务字段可随规则演进而更新，而不触及外键级联。

统一开发体验
前后端接口全部用数值 ID，ORM（EF Core 等）自动迁移更友好。

后续扩展弹性
若将来再移动书架，改变学生学号，如本科生变成研究生，学号改变，借阅记录保留，仅需更新业务列，不影响主键及关联表。

## 3. 说明
- bookinfo主码+业务码不分离的原因:保持业务主键 (变更概率极低/全球唯一)：BookInfo(ISBN)
- 以ID结尾的码是主码，不含业务逻辑