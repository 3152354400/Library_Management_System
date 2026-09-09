CREATE OR REPLACE VIEW BorrowRecordDetailView AS
SELECT
    br.BookID         AS BookId,
    bi.ISBN           AS ISBN,
    bi.Title          AS BookTitle,
    bi.Author         AS BookAuthor,
    r.ReaderID        AS ReaderId,
    r.Fullname        AS ReaderName,
    br.BorrowTime     AS BorrowTime,
    br.ReturnTime     AS ReturnTime,
    br.OverdueFine    AS OverdueFine
FROM BorrowRecord br
JOIN Reader r   ON br.ReaderID = r.ReaderID
JOIN Book b     ON br.BookID   = b.BookID
JOIN BookInfo bi ON b.ISBN     = bi.ISBN;