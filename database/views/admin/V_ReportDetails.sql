CREATE OR REPLACE VIEW V_ReportDetails AS
SELECT 
    r.ReportID,
    r.ReportReason,
    r.ReportTime,
    r.Status AS ReportStatus,
    
    ct.CommentID,
    ct.ReviewContent,
    ct.CreateTime AS CommentTime,
    
    commenter.ReaderID AS CommenterID,
    commenter.Nickname AS CommenterNickname,
    
    reporter.ReaderID AS ReporterID,
    reporter.Nickname AS ReporterNickname,
    
    bi.ISBN,
    bi.Title AS BookTitle
FROM 
    Report r
JOIN 
    Comment_Table ct ON r.CommentID = ct.CommentID
JOIN 
    Reader commenter ON ct.ReaderID = commenter.ReaderID
JOIN 
    Reader reporter ON r.ReaderID = reporter.ReaderID
JOIN 
    BookInfo bi ON ct.ISBN = bi.ISBN;

COMMENT ON TABLE V_ReportDetails IS '一个整合了举报、评论、评论者、举报者和图书信息的详细视图';