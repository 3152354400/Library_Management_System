public class CommentService
{
    private readonly CommentRepository _repository;

    public CommentService(CommentRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<CommentDetailDto>> SearchCommentAsync(string ISBN)
    {
        return _repository.SearchCommentAsync(ISBN);
    }
    
    public Task<IEnumerable<CommentDetailDto>> SearchCommentAsyncByCommentID(decimal comment_id)
    {
        return _repository.SearchCommentAsyncByCommentID(comment_id);
    }

    public async Task<int> AddCommentAsync(CommentDetailDto commentDto)
    {
        return await _repository.AddCommentAsync(commentDto);
    }
}


// public class ReportService
// {
//     private readonly ReportRepository _repository;

//     public ReportService(ReportRepository repository)
//     {
//         _repository = repository;
//     }

//     public async Task<IEnumerable<ReportDto>> GetReportsAsyncByReaderID(decimal reader_id)
//     {
//         return await _repository.GetReportsAsyncByReaderID(reader_id);
//     }

    
//     public async Task<IEnumerable<ReportDto>> GetReportsAsyncByLibrarianID(decimal librarian_id)
//     {
//         return await _repository.GetReportsAsyncByLibrarianID(librarian_id);
//     }

//     public async Task<int> AddReportAsync(ReportDto report)
//     {
//         return await _repository.AddReportAsync(report);
//     }
    
//     public async Task<int> ChangeStatusAsync(ReportStatusDto report_status)
//     {
//         return await _repository.ChangeStatusAsync(report_status);
//     }
// }