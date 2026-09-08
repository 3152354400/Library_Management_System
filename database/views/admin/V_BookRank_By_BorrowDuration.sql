CREATE OR REPLACE VIEW V_BookRank_By_BorrowDuration AS
SELECT 
    bi.ISBN,
    bi.Title,
    bi.Author,
    ROUND(SUM(CAST(br.ReturnTime AS DATE) - CAST(br.BorrowTime AS DATE))) AS MetricValue -- 聚合借阅记录表
FROM 
    BorrowRecord br -- 使用您的 BorrowRecord 表
JOIN 
    Book b ON br.BookID = b.BookID
JOIN 
    BookInfo bi ON b.ISBN = bi.ISBN
WHERE 
    br.ReturnTime IS NOT NULL -- 只计算已归还的记录
GROUP BY 
    bi.ISBN, bi.Title, bi.Author
ORDER BY 
    MetricValue DESC;