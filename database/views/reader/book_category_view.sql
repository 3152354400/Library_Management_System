CREATE OR REPLACE VIEW book_category_view AS
SELECT bc.ISBN, bc.CategoryID, c.CategoryName
FROM Book_Classify bc
         JOIN Category c ON bc.CategoryID = c.CategoryID;
