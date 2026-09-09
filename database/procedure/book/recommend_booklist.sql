CREATE OR REPLACE PROCEDURE RecommendBooklists(
    p_BooklistID IN Booklist.BooklistID%TYPE,
    p_Limit IN NUMBER DEFAULT 10,
    p_RecommendedBooklists OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_RecommendedBooklists FOR
    WITH CurrentBooklist AS (
        SELECT ISBN 
        FROM Booklist_Book 
        WHERE BooklistID = p_BooklistID
    ),
    OtherBooklists AS (
        SELECT b.BooklistID, 
               b.ListCode, 
               b.BooklistName,
               b.BooklistIntroduction,
               b.CreatorID,
               r.Nickname AS CreatorNickname,
               COUNT(bb.ISBN) AS TotalBooks,
               COUNT(CASE WHEN bb.ISBN IN (SELECT ISBN FROM CurrentBooklist) THEN 1 END) AS CommonBooks
        FROM Booklist b
        JOIN Booklist_Book bb ON b.BooklistID = bb.BooklistID
        JOIN Reader r ON b.CreatorID = r.ReaderID
        WHERE b.BooklistID != p_BooklistID
        GROUP BY b.BooklistID, b.ListCode, b.BooklistName, 
                 b.BooklistIntroduction, b.CreatorID, r.Nickname
    ),
    RankedBooklists AS (
        SELECT BooklistID,
               ListCode,
               BooklistName,
               BooklistIntroduction,
               CreatorID,
               CreatorNickname,
               CommonBooks AS MatchingBooksCount,
               ROUND(CommonBooks / (TotalBooks + (SELECT COUNT(*) FROM CurrentBooklist) - CommonBooks), 3) AS SimilarityScore,
               ROW_NUMBER() OVER (ORDER BY CommonBooks / (TotalBooks + (SELECT COUNT(*) FROM CurrentBooklist) - CommonBooks) DESC, CommonBooks DESC) AS rn
        FROM OtherBooklists
        WHERE CommonBooks > 0
    )
    SELECT BooklistID,
           ListCode,
           BooklistName,
           BooklistIntroduction,
           CreatorID,
           CreatorNickname,
           MatchingBooksCount,
           SimilarityScore
    FROM RankedBooklists
    WHERE rn <= p_Limit;
END;
/