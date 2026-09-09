CREATE OR REPLACE VIEW V_BookRank_By_InstanceBorrow AS
SELECT
    b.Barcode,
    bi.Title,
    bi.Author,
    COUNT(br.BorrowRecordID) AS MetricValue
FROM
    BorrowRecord br
JOIN
    Book b ON br.BookID = b.BookID
JOIN
    BookInfo bi ON b.ISBN = bi.ISBN
GROUP BY
    b.Barcode, bi.Title, bi.Author
ORDER BY
    MetricValue DESC;