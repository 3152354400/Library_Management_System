using System.Threading.Tasks;
using Backend.DTOs.Book;
using Backend.Repositories.Book;

namespace Backend.Services.Book
{
    public class BooklistService : IBooklistService
    {
        private readonly IBooklistRepository _repository;

        public BooklistService(IBooklistRepository repository)
        {
            _repository = repository;
        }

        public Task<BooklistSuccessResponse> AddBookToBooklistAsync(int booklistId, AddBookToBooklistRequest request, int readerId)
        {
            return _repository.AddBookToBooklistAsync(booklistId, request);

        }

        public Task<BooklistSuccessResponse> RemoveBookFromBooklistAsync(int booklistId, string isbn, int readerId)
        {
            return _repository.RemoveBookFromBooklistAsync(booklistId, isbn);
        }

        public Task<BooklistSuccessResponse> CollectBooklistAsync(int booklistId, int readerId, CollectBooklistRequest request)
        {
            return _repository.CollectBooklistAsync(booklistId, readerId, request);
        }

        public Task<BooklistSuccessResponse> CancelCollectBooklistAsync(int booklistId, int readerId)
        {
            return _repository.CancelCollectBooklistAsync(booklistId, readerId);
        }

        public Task<BooklistSuccessResponse> UpdateCollectNotesAsync(int booklistId, int readerId, UpdateCollectNotesRequest request)
        {
            return _repository.UpdateCollectNotesAsync(booklistId, readerId, request);
        }

        public Task<CreateBooklistResponse> CreateBooklistAsync(CreateBooklistRequest request, int creatorId)
        {
            return _repository.CreateBooklistAsync(request, creatorId);
        }

        public Task<BooklistSuccessResponse> DeleteBooklistAsync(int booklistId, int readerId)
        {
            return _repository.DeleteBooklistAsync(booklistId, readerId);
        }

        public Task<GetBooklistDetailsResponse?> GetBooklistDetailsAsync(int booklistId)
        {
            return _repository.GetBooklistDetailsAsync(booklistId);
        }

        public Task<RecommendBooklistsResponse> RecommendBooklistsAsync(int booklistId, int limit = 10)
        {
            return _repository.RecommendBooklistsAsync(booklistId, limit);
        }

        public Task<SearchBooklistsByReaderResponse> SearchBooklistsByReaderAsync(int readerId)
        {
            return _repository.SearchBooklistsByReaderAsync(readerId);
        }

        public Task<BooklistSuccessResponse> UpdateBooklistNameAsync(int booklistId, int readerId, UpdateBooklistNameRequest request)
        {
            return _repository.UpdateBooklistNameAsync(booklistId, readerId, request);
        }

        public Task<BooklistSuccessResponse> UpdateBooklistIntroAsync(int booklistId, int readerId, UpdateBooklistIntroRequest request)
        {
            return _repository.UpdateBooklistIntroAsync(booklistId, readerId, request);
        }
    }
}
