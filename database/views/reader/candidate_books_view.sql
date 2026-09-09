CREATE OR REPLACE VIEW candidate_books_view AS
SELECT DISTINCT bi.ReaderID, bc.ISBN
FROM borrowed_ISBNs_view bi
         JOIN Book_Classify bc
              ON bc.CategoryID IN (
                  SELECT bc2.CategoryID
                  FROM borrowed_ISBNs_view bi2
                           JOIN Book_Classify bc2 ON bi2.ISBN = bc2.ISBN
                  WHERE bi2.ReaderID = bi.ReaderID
              )
WHERE NOT EXISTS (
    SELECT 1
    FROM borrowed_ISBNs_view bi3
    WHERE bi3.ReaderID = bi.ReaderID
      AND bi3.ISBN = bc.ISBN
);
