CREATE OR REPLACE VIEW book_detail_view AS
SELECT
    b.BookID,
    b.Status,
    b.ShelfID,
    b.BuildingID,
    b.ISBN,
    bi.Title,
    bi.Author,
    bi.Stock
FROM Book b
JOIN BookInfo bi ON b.ISBN = bi.ISBN;