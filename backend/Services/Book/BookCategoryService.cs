using backend.DTOs.Book;
using backend.Models;
using backend.Repositories.Book;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace backend.Services.Book
{
    /// <summary>
    /// 图书分类关联业务逻辑层
    /// </summary>
    public class BookCategoryService
    {
        private readonly BookCategoryRepository _bookCategoryRepository;
        private readonly BookCategoryTreeOperation _categoryTreeOperation;
        private readonly LogService _logService;

        public BookCategoryService(
            BookCategoryRepository bookCategoryRepository,
            BookCategoryTreeOperation categoryTreeOperation,
            LogService logService)
        {
            _bookCategoryRepository = bookCategoryRepository;
            _categoryTreeOperation = categoryTreeOperation;
            _logService = logService;
        }

        /// <summary>
        /// 绑定图书到分类
        /// </summary>
        public async Task<bool> BindBookToCategoriesAsync(BookCategoryBindDto bindDto, string operatorId)
        {
            try
            {
                // 验证图书是否存在
                if (!await BookExistsAsync(bindDto.ISBN))
                {
                    throw new InvalidOperationException($"图书 ISBN {bindDto.ISBN} 不存在");
                }

                // 验证所有分类是否为叶子节点
                foreach (var categoryId in bindDto.CategoryIDs)
                {
                    if (!await _bookCategoryRepository.IsLeafCategoryAsync(categoryId))
                    {
                        var category = await _categoryTreeOperation.GetCategoryByIdAsync(categoryId);
                        var categoryName = category?.CategoryName ?? categoryId;
                        throw new InvalidOperationException($"分类 '{categoryName}' 不是叶子节点，无法绑定图书");
                    }
                }

                // 先删除现有的分类关联
                await _bookCategoryRepository.RemoveAllBookCategoriesAsync(bindDto.ISBN);

                // 添加新的分类关联
                var bookCategories = bindDto.CategoryIDs.Select(categoryId => new BookCategory
                {
                    ISBN = bindDto.ISBN,
                    CategoryID = categoryId,
                    RelationNote = bindDto.RelationNote
                }).ToList();

                if (bookCategories.Any())
                {
                    await _bookCategoryRepository.AddBookCategoriesAsync(bookCategories);
                }

                // 记录操作日志
                var bookTitle = await GetBookTitleAsync(bindDto.ISBN);
                var categoryNames = new List<string>();
                foreach (var categoryId in bindDto.CategoryIDs)
                {
                    var categoryPath = await GetCategoryPathAsync(categoryId);
                    categoryNames.Add(categoryPath);
                }

                var operationContent = $"将图书《{bookTitle}》归入分类：{string.Join("、", categoryNames)}";
                await _logService.LogOperationSuccessAsync(operationContent, "Librarian", operatorId);

                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogOperationFailureAsync("绑定图书分类", "Librarian", operatorId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 添加单个图书分类关联
        /// </summary>
        public async Task<bool> AddBookCategoryAsync(BookCategoryRequestDto requestDto, string operatorId)
        {
            try
            {
                // 验证图书是否存在
                if (!await BookExistsAsync(requestDto.ISBN))
                {
                    throw new InvalidOperationException($"图书 ISBN {requestDto.ISBN} 不存在");
                }

                // 验证分类是否为叶子节点
                if (!await _bookCategoryRepository.IsLeafCategoryAsync(requestDto.CategoryID))
                {
                    var category = await _categoryTreeOperation.GetCategoryByIdAsync(requestDto.CategoryID);
                    var categoryName = category?.CategoryName ?? requestDto.CategoryID;
                    throw new InvalidOperationException($"分类 '{categoryName}' 不是叶子节点，无法绑定图书");
                }

                // 检查关联是否已存在
                if (await _bookCategoryRepository.ExistsBookCategoryAsync(requestDto.ISBN, requestDto.CategoryID))
                {
                    throw new InvalidOperationException("该图书已关联到此分类");
                }

                // 添加关联
                var bookCategory = new BookCategory
                {
                    ISBN = requestDto.ISBN,
                    CategoryID = requestDto.CategoryID,
                    RelationNote = requestDto.RelationNote
                };

                await _bookCategoryRepository.AddBookCategoryAsync(bookCategory);

                // 记录操作日志
                var bookTitle = await GetBookTitleAsync(requestDto.ISBN);
                var categoryPath = await GetCategoryPathAsync(requestDto.CategoryID);
                var operationContent = $"将图书《{bookTitle}》归入分类：{categoryPath}";
                await _logService.LogOperationSuccessAsync(operationContent, "Librarian", operatorId);

                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogOperationFailureAsync("添加图书分类关联", "Librarian", operatorId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 移除图书分类关联
        /// </summary>
        public async Task<bool> RemoveBookCategoryAsync(string isbn, string categoryId, string operatorId)
        {
            try
            {
                Console.WriteLine($"开始移除图书分类关联: ISBN='{isbn}', CategoryID='{categoryId}', OperatorID='{operatorId}'");
                
                // 检查关联是否存在
                var exists = await _bookCategoryRepository.ExistsBookCategoryAsync(isbn, categoryId);
                Console.WriteLine($"检查关联存在性: {exists}");
                
                if (!exists)
                {
                    Console.WriteLine($"关联不存在: ISBN='{isbn}', CategoryID='{categoryId}'");
                    throw new InvalidOperationException("该图书分类关联不存在");
                }

                // 移除关联
                Console.WriteLine("开始移除关联");
                await _bookCategoryRepository.RemoveBookCategoryAsync(isbn, categoryId);
                Console.WriteLine("关联移除成功");

                // 记录操作日志
                var bookTitle = await GetBookTitleAsync(isbn);
                var categoryPath = await GetCategoryPathAsync(categoryId);
                var operationContent = $"将图书《{bookTitle}》从分类 {categoryPath} 中移除";
                await _logService.LogOperationSuccessAsync(operationContent, "Librarian", operatorId);

                Console.WriteLine("移除操作完成");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"移除操作失败: {ex.Message}");
                Console.WriteLine($"异常堆栈: {ex.StackTrace}");
                await _logService.LogOperationFailureAsync("移除图书分类关联", "Librarian", operatorId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 获取图书的所有分类关联
        /// </summary>
        public async Task<IEnumerable<BookCategoryDetailDto>> GetBookCategoriesAsync(string isbn)
        {
            return await _bookCategoryRepository.GetBookCategoriesAsync(isbn);
        }

        /// <summary>
        /// 获取分类的所有图书关联
        /// </summary>
        public async Task<IEnumerable<BookCategoryDetailDto>> GetCategoryBooksAsync(string categoryId)
        {
            return await _bookCategoryRepository.GetCategoryBooksAsync(categoryId);
        }

        /// <summary>
        /// 获取所有叶子节点分类（用于绑定选择）
        /// </summary>
        public async Task<IEnumerable<CategorySelectDto>> GetLeafCategoriesAsync()
        {
            return await _bookCategoryRepository.GetLeafCategoriesAsync();
        }

        /// <summary>
        /// 获取图书分类关联统计
        /// </summary>
        public async Task<Dictionary<string, int>> GetBookCategoryStatsAsync()
        {
            return await _bookCategoryRepository.GetBookCategoryStatsAsync();
        }

        /// <summary>
        /// 验证图书是否存在
        /// </summary>
        private async Task<bool> BookExistsAsync(string isbn)
        {
            var sql = "SELECT COUNT(*) FROM BookInfo WHERE ISBN = :ISBN";
            using var connection = new OracleConnection(_bookCategoryRepository.GetConnectionString());
            await connection.OpenAsync();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { ISBN = isbn });
            return count > 0;
        }

        /// <summary>
        /// 获取图书标题
        /// </summary>
        private async Task<string> GetBookTitleAsync(string isbn)
        {
            var sql = "SELECT Title FROM BookInfo WHERE ISBN = :ISBN";
            using var connection = new OracleConnection(_bookCategoryRepository.GetConnectionString());
            await connection.OpenAsync();
            var title = await connection.ExecuteScalarAsync<string>(sql, new { ISBN = isbn });
            return title ?? isbn;
        }

        /// <summary>
        /// 获取分类路径
        /// </summary>
        private async Task<string> GetCategoryPathAsync(string categoryId)
        {
            var path = await _categoryTreeOperation.GetCategoryPathAsync(categoryId);
            return string.Join(" / ", path);
        }
    }
}