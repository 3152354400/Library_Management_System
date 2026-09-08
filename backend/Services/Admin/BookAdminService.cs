using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTOs.Admin;
using backend.Repositories.Admin;
using System;

namespace backend.Services.Admin
{
    public class BookAdminService
    {
        private readonly BookAdminRepository _repository;

        public BookAdminService(BookAdminRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<BookAdminDto>> GetBooksAsync(string searchTerm)
        {
            return _repository.GetBooksAsync(searchTerm);
        }

        public async Task CreateBookAsync(CreateBookDto dto)
        {
            var exists = await _repository.IsIsbnExistsAsync(dto.ISBN);
            if (exists)
            {
                throw new InvalidOperationException("This ISBN already exists in the library.");
            }
            await _repository.CreateBookAsync(dto);
        }

        public Task<bool> UpdateBookInfoAsync(string isbn, UpdateBookDto dto)
        {
            return _repository.UpdateBookInfoAsync(isbn, dto);
        }

        public Task<bool> TakedownBookAsync(string isbn)
        {
            return _repository.TakedownBookAsync(isbn);
        }

        public async Task AddCopiesAsync(AddCopiesDto dto)
        {
            // 确保目标图书种类 (ISBN) 确实存在
            var exists = await _repository.IsIsbnExistsAsync(dto.ISBN);
            if (!exists)
            {
                throw new InvalidOperationException("无法为不存在的ISBN添加入库，请先新增该图书种类。");
            }
            await _repository.AddCopiesAsync(dto);
        }
    }
}