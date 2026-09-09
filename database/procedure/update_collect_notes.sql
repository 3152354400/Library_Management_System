CREATE OR REPLACE PROCEDURE UpdateCollectNotes(
    p_BooklistID IN Collect.BooklistID%TYPE,
    p_ReaderID   IN Collect.ReaderID%TYPE,
    p_NewNotes   IN Collect.Notes%TYPE,
    p_Success    OUT NUMBER
)
AS
BEGIN
    p_Success := 0;

    UPDATE Collect
    SET Notes = p_NewNotes
    WHERE BooklistID = p_BooklistID
      AND ReaderID = p_ReaderID;

    IF SQL%ROWCOUNT > 0 THEN
        p_Success := 1;
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE;
END;
/
