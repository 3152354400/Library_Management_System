using System.Threading.Tasks;
using Backend.DTOs.Book;

namespace Backend.Services.Book
{
    public interface IBooklistService
    {
        Task<BooklistSuccessResponse> AddBookToBooklistAsync(int booklistId, AddBookToBooklistRequest request, int readerId);
        Task<BooklistSuccessResponse> RemoveBookFromBooklistAsync(int booklistId, string isbn, int readerId);

        Task<BooklistSuccessResponse> CollectBooklistAsync(int booklistId, int readerId, CollectBooklistRequest request);
        Task<BooklistSuccessResponse> CancelCollectBooklistAsync(int booklistId, int readerId);
        Task<BooklistSuccessResponse> UpdateCollectNotesAsync(int booklistId, int readerId, UpdateCollectNotesRequest request);

        Task<CreateBooklistResponse> CreateBooklistAsync(CreateBooklistRequest request, int creatorId);
        Task<BooklistSuccessResponse> DeleteBooklistAsync(int booklistId, int readerId);

        Task<GetBooklistDetailsResponse?> GetBooklistDetailsAsync(int booklistId);
        Task<RecommendBooklistsResponse> RecommendBooklistsAsync(int booklistId, int limit = 10);

        Task<SearchBooklistsByReaderResponse> SearchBooklistsByReaderAsync(int readerId);

        Task<BooklistSuccessResponse> UpdateBooklistNameAsync(int booklistId, int readerId, UpdateBooklistNameRequest request);
        Task<BooklistSuccessResponse> UpdateBooklistIntroAsync(int booklistId, int readerId, UpdateBooklistIntroRequest request);
    }
}
