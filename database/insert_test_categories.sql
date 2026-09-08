-- 插入测试分类数据
-- 请先执行此脚本来创建测试数据

-- 清空现有数据（可选）
-- DELETE FROM Book_Classify;
-- DELETE FROM Category;

-- 插入分类数据
INSERT INTO Category (CategoryID, CategoryName, ParentCategoryID) VALUES ('C001', '文学', NULL);
INSERT INTO Category (CategoryID, CategoryName, ParentCategoryID) VALUES ('C002', '哲学', NULL);
INSERT INTO Category (CategoryID, CategoryName, ParentCategoryID) VALUES ('C003', '科学', NULL);
INSERT INTO Category (CategoryID, CategoryName, ParentCategoryID) VALUES ('C004', '小说', 'C001');
INSERT INTO Category (CategoryID, CategoryName, ParentCategoryID) VALUES ('C005', '诗歌', 'C001');
INSERT INTO Category (CategoryID, CategoryName, ParentCategoryID) VALUES ('C006', '西方哲学', 'C002');
INSERT INTO Category (CategoryID, CategoryName, ParentCategoryID) VALUES ('C007', '东方哲学', 'C002');

-- 插入图书分类关联数据（如果有图书数据）
-- INSERT INTO Book_Classify (ISBN, CategoryID, RelationNote) VALUES ('9780140449136', 'C004', '经典小说');
-- INSERT INTO Book_Classify (ISBN, CategoryID, RelationNote) VALUES ('9780140449136', 'C001', '文学类');

-- 提交事务
COMMIT;

-- 验证数据
SELECT 'Category表记录数:' as Info, COUNT(*) as Count FROM Category;
SELECT 'Book_Classify表记录数:' as Info, COUNT(*) as Count FROM Book_Classify;
