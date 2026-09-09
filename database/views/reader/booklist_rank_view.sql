CREATE OR REPLACE VIEW book_list_rank_view AS
SELECT
    cb.ReaderID,
    cb.ISBN,
    NVL(COUNT(bb.BooklistID), 0) AS BooklistCount
FROM candidate_books_view cb
         LEFT JOIN Booklist_Book bb ON cb.ISBN = bb.ISBN
GROUP BY cb.ReaderID, cb.ISBN;
