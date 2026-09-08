public class CommentRepository
{
    private readonly string _connectionString;

    public CommentRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<CommentDetailDto>> SearchCommentAsync(string ISBN)
    {
        var sql = @"
            SELECT *
            FROM COMMENT_TABLE
            WHERE ISBN = :ISBN and status = '正常'";

        using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
        await connection.OpenAsync();

        return await Dapper.SqlMapper.QueryAsync<CommentDetailDto>(connection, sql, new { ISBN = ISBN });
    }

    
    public async Task<IEnumerable<CommentDetailDto>> SearchCommentAsyncByCommentID(decimal comment_id)
    {
        var sql = @"
            SELECT *
            FROM COMMENT_TABLE
            WHERE commentID = :comment_id";

        using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
        await connection.OpenAsync();

        // Console.WriteLine($"id = {comment_id}");
        return await Dapper.SqlMapper.QueryAsync<CommentDetailDto>(connection, sql, new { comment_id = comment_id });
    }
    
    public async Task<int> AddCommentAsync(CommentDetailDto commentDto)
    {
        var sql = @"
            INSERT INTO Comment_Table (READERID, ISBN, RATING, REVIEWCONTENT, CREATETIME, STATUS)
            VALUES (:ReaderID, :ISBN, :Rating, :REVIEWCONTENT, :CREATETIME, :Status)";

        using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
        await connection.OpenAsync();
        
        return await Dapper.SqlMapper.ExecuteAsync(connection, sql, commentDto);
    }
}

// public class ReportRepository
// {
//     private readonly string _connectionString;

//     public ReportRepository(string connectionString)
//     {
//         _connectionString = connectionString;
//     }

//     public async Task<IEnumerable<ReportDto>> GetReportsAsyncByReaderID(decimal reader_id)
//     {
//         var sql = @"
//             SELECT *
//             FROM REPORT
//             WHERE READERID = :reader_id";

//         using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
//         await connection.OpenAsync();

//         return await Dapper.SqlMapper.QueryAsync<ReportDto>(connection, sql, new { reader_id = reader_id });
//     }
    // 

//     public async Task<IEnumerable<ReportDto>> GetReportsAsyncByLibrarianID(decimal librarian_id)
//     {
//         var sql = @"
//             SELECT *
//             FROM REPORT
//             WHERE LIBRARIANID = :librarian_id and status = '待处理'";

//         using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
//         await connection.OpenAsync();

//         return await Dapper.SqlMapper.QueryAsync<ReportDto>(connection, sql, new { librarian_id = librarian_id });
//     }

//     public async Task<int> AddReportAsync(ReportDto report)
//     {
//         var sql = @"
//             INSERT INTO report(COMMENTID, READERID, REPORTREASON, REPORTTIME, STATUS, LIBRARIANID)
//             VALUES (:CommentID, :READERID, :ReportReason, :ReportTime, :Status, :LibrarianID)";

//         using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
//         await connection.OpenAsync();
        
//         return await Dapper.SqlMapper.ExecuteAsync(connection, sql, report);
//     }

//     public async Task<int> ChangeStatusAsync(ReportStatusDto report_status)
//     {
//         // 如果状态是处理完成，先删除相关评论
//         if (report_status.Status == "处理完成") {
//             // Console.WriteLine($"准备更新评论状态，ReportID: {report_status.ReportID}");
//             var delete_comment_sql = @"update comment_table
//                                         set status = '已删除'
//                                         where commentid = (select commentID 
//                                                         from report 
//                                                         where reportID = :ReportID)";


//             using var delete_comment_connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
//             await delete_comment_connection.OpenAsync();
//             // Console.WriteLine($"执行SQL: {delete_comment_sql}, 参数: ReportID={report_status.ReportID}");
//             await Dapper.SqlMapper.ExecuteAsync(delete_comment_connection, delete_comment_sql, new { 
//             ReportID = report_status.ReportID  });
//             // Console.WriteLine("评论状态更新完成");
//         }

//         // 更新报告状态
//         var sql = @"
//             UPDATE report
//             SET STATUS = :Status
//             WHERE REPORTID = :ReportID";

//         using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(_connectionString);
//         await connection.OpenAsync();

//         // Console.WriteLine($"执行SQL: {sql}, 参数: Status={report_status.Status}, ReportID={report_status.ReportID}");
//         var result = await Dapper.SqlMapper.ExecuteAsync(connection, sql, new { 
//             Status = report_status.Status, 
//             ReportID = report_status.ReportID 
//         });
//         // Console.WriteLine($"报告状态更新完成，影响行数: {result}");
//         return result;
//     }
// }