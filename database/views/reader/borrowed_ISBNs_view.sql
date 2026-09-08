CREATE OR REPLACE VIEW borrowed_ISBNs_view AS
SELECT DISTINCT br.ReaderID, b.ISBN
FROM BorrowRecord br
         JOIN Book b ON br.BookID = b.BookID;
