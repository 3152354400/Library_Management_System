CREATE OR REPLACE VIEW V_BookRank_By_BorrowCount AS
SELECT 
    bi.ISBN,
    bi.Title,
    bi.Author,
    COUNT(br.BorrowRecordID) AS MetricValue -- 聚合借阅记录表
FROM 
    BorrowRecord br -- 使用您的 BorrowRecord 表
JOIN 
    Book b ON br.BookID = b.BookID
JOIN 
    BookInfo bi ON b.ISBN = bi.ISBN
GROUP BY 
    bi.ISBN, bi.Title, bi.Author
ORDER BY 
    MetricValue DESC;