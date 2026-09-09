CREATE OR REPLACE PROCEDURE UpdateBooklistName(
    p_BooklistID IN Booklist.BooklistID%TYPE,
    p_ReaderID   IN Reader.ReaderID%TYPE,
    p_NewName    IN Booklist.BooklistName%TYPE,
    p_Success    OUT NUMBER
)
AS
    v_CreatorID Booklist.CreatorID%TYPE;
BEGIN
    p_Success := 0;

    SELECT CreatorID INTO v_CreatorID
    FROM Booklist
    WHERE BooklistID = p_BooklistID;

    IF v_CreatorID = p_ReaderID THEN
        UPDATE Booklist
        SET BooklistName = p_NewName
        WHERE BooklistID = p_BooklistID;

        IF SQL%ROWCOUNT > 0 THEN
            p_Success := 1;
        END IF;
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_Success := 0;
    WHEN OTHERS THEN
        RAISE;
END;
/

CREATE OR REPLACE PROCEDURE UpdateBooklistIntro(
    p_BooklistID IN Booklist.BooklistID%TYPE,
    p_ReaderID   IN Reader.ReaderID%TYPE,
    p_NewIntro   IN Booklist.BooklistIntroduction%TYPE,
    p_Success    OUT NUMBER
)
AS
    v_CreatorID Booklist.CreatorID%TYPE;
BEGIN
    p_Success := 0;

    SELECT CreatorID INTO v_CreatorID
    FROM Booklist
    WHERE BooklistID = p_BooklistID;

    IF v_CreatorID = p_ReaderID THEN
        UPDATE Booklist
        SET BooklistIntroduction = p_NewIntro
        WHERE BooklistID = p_BooklistID;

        IF SQL%ROWCOUNT > 0 THEN
            p_Success := 1;
        END IF;
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_Success := 0;
    WHEN OTHERS THEN
        RAISE;
END;
/
