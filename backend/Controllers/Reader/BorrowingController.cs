using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services.BorrowingService;
using backend.Services.ReaderService;
using backend.DTOs;
using backend.Services.Web;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BorrowingController : ControllerBase
    {
        private readonly BorrowingService _borrowingService;

        private readonly SecurityService _securityService;

        /**
         * 构造函数
         * @param borrowingService BorrowingService 实例
         * @return 无
         */
        public BorrowingController(BorrowingService borrowingService, SecurityService securityService)
        {
            _borrowingService = borrowingService;
            _securityService = securityService;
        }

        /**
         * 获取所有借阅记录
         * @return 借阅记录列表
         */
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BorrowRecordDetailDto>>> GetAllBorrowRecords()
        {
            var borrowRecords = await _borrowingService.GetAllBorrowRecordsAsync();
            return Ok(borrowRecords);
        }

        /**
         * 通过借阅记录ID获取借阅记录
         * @param id 借阅记录ID
         * @return 借阅记录详情
         */
        [HttpGet("{id}")]
        public async Task<ActionResult<BorrowRecordDetailDto>> GetBorrowRecordByID(int id)
        {
            var borrowRecord = await _borrowingService.GetBorrowRecordByIDAsync(id);
            if (borrowRecord == null) return NotFound();
            return Ok(borrowRecord);
        }

        /**
         * 通过读者ID获取该读者的所有借阅记录
         * @param readerID 读者ID
         * @return 该读者的借阅记录列表
         */
        [HttpGet("reader")]
        public async Task<ActionResult<IEnumerable<BorrowRecordDetailDto>>> GetBorrowRecordsByReaderID()
        {
            var loginUser = _securityService.GetLoginUser();

            if (!_securityService.CheckIsReader(loginUser))
            {
                return Forbid(); // 或 return Unauthorized();
            }

            var reader = loginUser.User as Reader;
            if (reader == null)
            {
                return BadRequest("当前用户不是读者");
            }

            var borrowRecords = await _borrowingService.GetBorrowRecordsByReaderIDAsync(reader.ReaderID.ToString());
            return Ok(borrowRecords);
        }

        /**
         * 通过图书ID获取所有借阅该图书的记录
         * @param bookID 图书ID
         * @return 借阅该图书的记录列表
         */
        [HttpGet("book/{bookID}")]
        public async Task<ActionResult<IEnumerable<BorrowRecordDetailDto>>> GetBorrowRecordsByBookID(string bookID)
        {
            var borrowRecords = await _borrowingService.GetBorrowRecordsByBookIDAsync(bookID);
            return Ok(borrowRecords);
        }

        /**
         * 添加新的借阅记录
         * @param dto BorrowRecordDto
         * @return 业务处理结果
         */
        [HttpPost]
        public async Task<ActionResult> AddBorrowRecord([FromBody] BorrowRecordDto dto)
        {
            if (dto == null)
            {
                return BadRequest("输入数据不能为空");
            }

            var borrowRecord = new BorrowRecord
            {
                BookId = dto.BookId,
                ReaderId = dto.ReaderId,
                BorrowTime = dto.BorrowTime,
                DueTime = dto.DueTime,
                ReturnTime = dto.ReturnTime,
                OverdueFine = dto.OverdueFine,
                Status = dto.Status
            };

            try
            {
                var result = await _borrowingService.AddBorrowRecordAsync(borrowRecord);
                return result > 0 ? Ok("借阅记录添加成功") : BadRequest("借阅记录添加失败");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"服务器错误: {ex.Message}");
            }
        }

        /**
         * 更新借阅记录
         * @param dto BorrowRecordDto
         * @return 业务处理结果
         */
        [HttpPut]
        public async Task<ActionResult> UpdateBorrowRecord([FromBody] BorrowRecordDto dto)
        {
            if (dto == null)
            {
                return BadRequest("输入数据不能为空");
            }

            var borrowRecord = new BorrowRecord
            {
                BorrowRecordID = dto.BorrowRecordID,
                BookId = dto.BookId,
                ReaderId = dto.ReaderId,
                BorrowTime = dto.BorrowTime,
                DueTime = dto.DueTime,
                ReturnTime = dto.ReturnTime,
                OverdueFine = dto.OverdueFine,
                Status = dto.Status
            };

            try
            {
                var result = await _borrowingService.UpdateBorrowRecordAsync(borrowRecord);
                return result > 0 ? Ok("借阅记录更新成功") : NotFound("借阅记录未找到");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"服务器错误: {ex.Message}");
            }
        }

        /**
        * 归还图书接口
        * @param readerId 读者ID（从查询参数获取）
        * @param bookId 图书ID（从查询参数获取）
        * @return 返回 IActionResult，成功返回200 OK，业务异常返回409 Conflict，其他异常返回500
        */
        [HttpPost("return")]
        public async Task<IActionResult> ReturnBook([FromQuery] string readerId, [FromQuery] string bookId)
        {
            try
            {
                // 调用服务层归还方法，成功返回提示信息
                var message = await _borrowingService.ReturnBookAsync(readerId, bookId);

                // 成功返回 200 OK
                return Ok(new { message });
            }
            catch (InvalidOperationException ex)
            {
                // 出现业务异常返回 409 Conflict
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // 其他未处理异常返回 500 Internal Server Error
                return StatusCode(500, new { message = "服务器发生错误：" + ex.Message });
            }
        }

        /**
         * 借阅图书接口
         * @param readerId 读者ID
         * @param bookId 图书ID
         * @return 借阅操作结果
         */
        [HttpPost("borrow")]  // 与前端请求路径匹配，使用POST方法
        public async Task<IActionResult> BorrowBook([FromQuery] string readerId, [FromQuery] string bookId)
        {
            // 调用服务层的借阅方法
            var response = await _borrowingService.BorrowBookAsync(readerId, bookId);

            if (response.Success)
            {
                // 成功返回200 OK
                return Ok(response);
            }
            else
            {
                // 失败返回400 Bad Request并包含错误信息
                return BadRequest(response);
            }
        }

        /**
         * 通过借阅记录ID删除借阅记录
         * @param id 借阅记录ID
         * @return 业务处理结果
         */
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBorrowRecord(string id)
        {
            try
            {
                var result = await _borrowingService.DeleteBorrowRecordAsync(id);
                return result > 0 ? Ok("借阅记录删除成功") : NotFound("借阅记录未找到");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"服务器错误: {ex.Message}");
            }
        }

        /**
        * 获取当前读者未归还书籍数量
        * @param readerId 读者 ID
        * @return 未归还书籍数量
        */
        [HttpGet("unreturned-count/{readerId}")]
        public async Task<ActionResult<int>> GetUnreturnedCountByReader(string readerId)
        {
            var loginUser = _securityService.GetLoginUser();

            if (!_securityService.CheckIsReader(loginUser))
            {
                return Forbid(); // 或 return Unauthorized();
            }

            var reader = loginUser.User as Reader;
            if (reader == null)
            {
                return BadRequest("当前用户不是读者");
            }

            readerId = reader.ReaderID.ToString();

            var count = await _borrowingService.GetUnreturnedCountByReaderAsync(readerId);

            if (count < 0)
            {
                return NotFound("未找到该读者或数据异常");
            }

            return Ok(count);
        }

        /**
        * 获取当前读者未归还且逾期书籍数量
        * @param readerId 读者 ID
        * @return 未归还书籍数量
        */
        [HttpGet("overdue-unreturned-count/{readerId}")]
        public async Task<ActionResult<int>> GetOverdueUnreturnedCountByReader(string readerId)
        {
            var loginUser = _securityService.GetLoginUser();

            if (!_securityService.CheckIsReader(loginUser))
            {
                return Forbid(); // 或 return Unauthorized();
            }

            var reader = loginUser.User as Reader;
            if (reader == null)
            {
                return BadRequest("当前用户不是读者");
            }

            readerId = reader.ReaderID.ToString();

            var count = await _borrowingService.GetOverdueUnreturnedAndOverdueCountByReaderAsync(readerId);

            if (count < 0)
            {
                return NotFound("未找到该读者或数据异常");
            }

            return Ok(count);
        }
        
        /**
        * 获取当前读者所有未归还且逾期书籍数量
        * @param readerId 读者 ID
        * @return 未归还书籍数量
        */
        [HttpGet("all-overdue-unreturned-count/{readerId}")]
        public async Task<ActionResult<int>> GetAllOverdueUnreturnedCountByReader(string readerId)
        {
            var loginUser = _securityService.GetLoginUser();

            if (!_securityService.CheckIsReader(loginUser))
            {
                return Forbid(); // 或 return Unauthorized();
            }

            var reader = loginUser.User as Reader;
            if (reader == null)
            {
                return BadRequest("当前用户不是读者");
            }

            readerId = reader.ReaderID.ToString();

            var count = await _borrowingService.GetALlOverdueUnreturnedAndOverdueCountByReaderAsync(readerId);

            if (count < 0)
            {
                return NotFound("未找到该读者或数据异常");
            }

            return Ok(count);
        }
    }
}
