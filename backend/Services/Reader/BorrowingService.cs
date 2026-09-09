using backend.DTOs;
using backend.Models;
using backend.Repositories.BorrowRecordRepository;

namespace backend.Services.BorrowingService
{
    public class BorrowingService
    {
        private readonly BorrowRecordRepository _borrowRecordRepository;

        // 构造函数
        public BorrowingService(BorrowRecordRepository borrowRecordRepository)
        {
            _borrowRecordRepository = borrowRecordRepository;
        }

        // 异步方法，通过借阅记录ID获取借阅记录
        public async Task<BorrowRecordDetailDto?> GetBorrowRecordByIDAsync(int borrowRecordID)
        {
            return await _borrowRecordRepository.GetByIDAsync(borrowRecordID);
        }

        // 异步方法，通过读者ID获取该读者的所有借阅记录
        public async Task<IEnumerable<BorrowRecordDetailDto>> GetBorrowRecordsByReaderIDAsync(string readerID)
        {
            return await _borrowRecordRepository.GetByReaderIDAsync(readerID);
        }

        // 异步方法，通过图书ID获取所有借阅该图书的记录
        public async Task<IEnumerable<BorrowRecordDetailDto>> GetBorrowRecordsByBookIDAsync(string bookID)
        {
            return await _borrowRecordRepository.GetByBookIDAsync(bookID);
        }

        // 异步方法，获取所有的借阅记录
        public async Task<IEnumerable<BorrowRecordDetailDto>> GetAllBorrowRecordsAsync()
        {
            return await _borrowRecordRepository.GetAllBorrowRecordsAsync();
        }

        // 异步方法，图书借阅功能
        public async Task<BorrowingServiceResponse<string>> BorrowBookAsync(string readerId, string bookId)
        {
            // 1. 参数验证
            if (string.IsNullOrEmpty(readerId) || string.IsNullOrEmpty(bookId))
            {
                throw new InvalidOperationException("读者ID和图书ID不能为空");
            }

            // 2. 借阅业务由数据库存储过程完成账户、信用分、未归还数量和图书状态校验
            await _borrowRecordRepository.BorrowBookAsync(readerId, bookId);

            // 3. 返回成功响应
            return new BorrowingServiceResponse<string>
            {
                Success = true,
                Message = "图书借阅成功",
                Data = $"读者 {readerId} 成功借阅图书 {bookId}，借阅时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}"
            };
        }


        /**
        * 异步方法，归还图书
        * @param readerId 读者ID
        * @param bookId 图书ID
        * @return 返回归还成功的提示信息
        * @throws InvalidOperationException 当参数无效、重复归还、记录不存在或其他错误时抛出
        */
        public async Task<string> ReturnBookAsync(string readerId, string bookId)
        {
            // 1. 参数验证
            if (string.IsNullOrEmpty(readerId) || string.IsNullOrEmpty(bookId))
            {
                throw new InvalidOperationException("读者ID和图书ID不能为空");
            }

            // 2. 调用仓库层的归还方法
            var result = await _borrowRecordRepository.ReturnBookAsync(readerId, bookId);

            // 3. 根据仓库返回结果进行判断
            return result switch
            {
                1 => $"读者 {readerId} 已成功归还图书 {bookId}",

                0 => throw new InvalidOperationException(
                    $"读者 {readerId} 已归还过图书 {bookId}，不能重复归还"),

                -1 => throw new InvalidOperationException(
                    $"未找到读者 {readerId} 借阅图书 {bookId} 的记录"),

                _ => throw new InvalidOperationException("归还图书时发生未知错误")
            };
        }

        // 异步方法，添加新的借阅记录
        public async Task<int> AddBorrowRecordAsync(BorrowRecord borrowRecord)
        {
            return await _borrowRecordRepository.AddAsync(borrowRecord);
        }

        // 异步方法，更新借阅记录
        public async Task<int> UpdateBorrowRecordAsync(BorrowRecord borrowRecord)
        {
            return await _borrowRecordRepository.UpdateAsync(borrowRecord);
        }

        // 异步方法，通过借阅记录ID删除借阅记录
        public async Task<int> DeleteBorrowRecordAsync(string borrowRecordID)
        {
            return await _borrowRecordRepository.DeleteAsync(borrowRecordID);
        }

        // 新增：通过读者ID获取该读者的所有借阅记录并转换为DTO
        public async Task<BorrowingServiceResponse<List<MyBorrowRecordDto>>> GetMyBorrowRecordDtosByReaderIdAsync(string readerId)
        {
            if (string.IsNullOrEmpty(readerId))
            {
                return new BorrowingServiceResponse<List<MyBorrowRecordDto>>
                {
                    Success = false,
                    Message = "读者ID不能为空",
                    Data = new List<MyBorrowRecordDto>()
                };
            }

            try
            {
                var records = await _borrowRecordRepository.GetMyBorrowRecordDtosByReaderIdAsync(readerId);
                return new BorrowingServiceResponse<List<MyBorrowRecordDto>>
                {
                    Success = true,
                    Data = records,
                    Message = records.Any() ? "查询成功" : "未找到借阅记录"
                };
            }
            catch (Exception ex)
            {
                return new BorrowingServiceResponse<List<MyBorrowRecordDto>>
                {
                    Success = false,
                    Message = $"查询失败：{ex.Message}",
                    Data = new List<MyBorrowRecordDto>()
                };
            }
        }

        /**
        * 获取指定读者当前未归还书籍数量
        * @param readerId 读者 ID
        * @return 未归还书籍数量
        */
        public async Task<int> GetUnreturnedCountByReaderAsync(string readerId)
        {
            return await _borrowRecordRepository.GetUnreturnedCountByReaderAsync(readerId);
        }

        /**
        * 获取指定读者当前未归还且逾期书籍数量
        * @param readerId 读者 ID
        * @return 当前未归还且逾期书籍数量
        */
        public async Task<int> GetOverdueUnreturnedAndOverdueCountByReaderAsync(string readerId)
        {
            return await _borrowRecordRepository.GetOverdueUnreturnedCountByReaderAsync(readerId);
        }
        
        /**
        * 获取指定读者所有未归还且逾期书籍数量
        * @param readerId 读者 ID
        * @return 未归还书籍数量
        */
        public async Task<int> GetALlOverdueUnreturnedAndOverdueCountByReaderAsync(string readerId)
        {
            return await _borrowRecordRepository.GetAllOverdueCountByReaderAsync(readerId);
        }

        /// <summary>
        /// 获取读者借阅记录（分页）
        /// </summary>
        public async Task<PagedResult<MyBorrowRecordDto>> GetBorrowRecordsByReaderPagedAsync(
            string readerId, int pageSize, int pageNum, string status = null)
        {
            var (data, totalCount) = await _borrowRecordRepository.GetByReaderIdPagedAsync(readerId, pageSize, pageNum, status);
            return new PagedResult<MyBorrowRecordDto>
            {
                Data = data,
                TotalCount = totalCount,
                PageNum = pageNum,
                PageSize = pageSize
            };
        }
    }
    
    // 通用服务响应模型
    public class BorrowingServiceResponse<T>
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public required T Data { get; set; }
    }
}