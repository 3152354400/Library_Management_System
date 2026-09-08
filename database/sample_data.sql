-- 示例数据插入脚本
-- 用于测试图书搜索和分类显示功能

-- 1. 插入分类数据
INSERT INTO Category (CategoryID, CategoryName, ParentCategoryID) VALUES ('LIT001', '文学', NULL);
INSERT INTO Category (CategoryID, CategoryName, ParentCategoryID) VALUES ('TECH001', '技术', NULL);
INSERT INTO Category (CategoryID, CategoryName, ParentCategoryID) VALUES ('HIST001', '历史', NULL);
INSERT INTO Category (CategoryID, CategoryName, ParentCategoryID) VALUES ('PHIL001', '哲学', NULL);
INSERT INTO Category (CategoryID, CategoryName, ParentCategoryID) VALUES ('ART001', '艺术', NULL);

-- 2. 插入图书信息（如果不存在）
INSERT INTO BookInfo (ISBN, Title, Author, Stock) VALUES ('9780140449136', 'Crime and punishment', 'Фёдор Михайлович Достоевский', 5);
INSERT INTO BookInfo (ISBN, Title, Author, Stock) VALUES ('9780140449273', 'The Symposium', 'Πλάτων', 5);
INSERT INTO BookInfo (ISBN, Title, Author, Stock) VALUES ('9780140441185', 'Thus spoke Zarathustra', 'Friedrich Nietzsche', 5);
INSERT INTO BookInfo (ISBN, Title, Author, Stock) VALUES ('9780140447934', 'War and Peace', 'Лев Толстой', 5);
INSERT INTO BookInfo (ISBN, Title, Author, Stock) VALUES ('9780142437230', 'Don Quixote', 'Miguel de Cervantes', 5);

-- 3. 插入图书分类关联
INSERT INTO Book_Classify (ISBN, CategoryID, RelationNote) VALUES ('9780140449136', 'LIT001', '经典文学');
INSERT INTO Book_Classify (ISBN, CategoryID, RelationNote) VALUES ('9780140449273', 'PHIL001', '哲学经典');
INSERT INTO Book_Classify (ISBN, CategoryID, RelationNote) VALUES ('9780140441185', 'PHIL001', '哲学思想');
INSERT INTO Book_Classify (ISBN, CategoryID, RelationNote) VALUES ('9780140447934', 'LIT001', '俄国文学');
INSERT INTO Book_Classify (ISBN, CategoryID, RelationNote) VALUES ('9780142437230', 'LIT001', '西班牙文学');

COMMIT;

-- 4. 验证数据
SELECT 'Category表记录数:' as Info, COUNT(*) as Count FROM Category
UNION ALL
SELECT 'BookInfo表记录数:' as Info, COUNT(*) as Count FROM BookInfo
UNION ALL
SELECT 'Book_Classify表记录数:' as Info, COUNT(*) as Count FROM Book_Classify;
