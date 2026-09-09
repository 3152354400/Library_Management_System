using backend.DTOs.Book;
using backend.Services.Book;
using backend.Services.Web;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Book
{
    /// <summary>
    /// 图书分类关联控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BookCategoryController : ControllerBase
    {
        private readonly BookCategoryService _bookCategoryService;
        private readonly SecurityService _securityService;

        public BookCategoryController(BookCategoryService bookCategoryService, SecurityService securityService)
        {
            _bookCategoryService = bookCategoryService;
            _securityService = securityService;
        }

        /// <summary>
        /// 绑定图书到多个分类
        /// </summary>
        /// <param name="bindDto">绑定请求</param>
        /// <returns>操作结果</returns>
        [HttpPost("bind")]
        public async Task<ActionResult> BindBookToCategories([FromBody] BookCategoryBindDto bindDto)
        {
            try
            {
                var loginUser = _securityService.GetLoginUser();
                var operatorId = loginUser.UserName;

                var result = await _bookCategoryService.BindBookToCategoriesAsync(bindDto, operatorId);
                return result ? Ok(new { message = "图书分类绑定成功" }) : BadRequest(new { message = "图书分类绑定失败" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
            }
        }

        /// <summary>
        /// 添加单个图书分类关联
        /// </summary>
        /// <param name="requestDto">关联请求</param>
        /// <returns>操作结果</returns>
        [HttpPost("add")]
        public async Task<ActionResult> AddBookCategory([FromBody] BookCategoryRequestDto requestDto)
        {
            try
            {
                var loginUser = _securityService.GetLoginUser();
                var operatorId = loginUser.UserName;

                var result = await _bookCategoryService.AddBookCategoryAsync(requestDto, operatorId);
                return result ? Ok(new { message = "图书分类关联添加成功" }) : BadRequest(new { message = "图书分类关联添加失败" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
            }
        }

        /// <summary>
        /// 移除图书分类关联
        /// </summary>
        /// <param name="isbn">图书ISBN</param>
        /// <param name="categoryId">分类ID</param>
        /// <returns>操作结果</returns>
        [HttpDelete("{isbn}/{categoryId}")]
        public async Task<ActionResult> RemoveBookCategory(string isbn, string categoryId)
        {
            try
            {
                var loginUser = _securityService.GetLoginUser();
                var operatorId = loginUser.UserName;

                var result = await _bookCategoryService.RemoveBookCategoryAsync(isbn, categoryId, operatorId);
                return result ? Ok(new { message = "图书分类关联移除成功" }) : BadRequest(new { message = "图书分类关联移除失败" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
            }
        }

        /// <summary>
        /// 获取图书的所有分类关联
        /// </summary>
        /// <param name="isbn">图书ISBN</param>
        /// <returns>分类关联列表</returns>
        [HttpGet("book/{isbn}")]
        public async Task<ActionResult<IEnumerable<BookCategoryDetailDto>>> GetBookCategories(string isbn)
        {
            try
            {
                var categories = await _bookCategoryService.GetBookCategoriesAsync(isbn);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
            }
        }

        /// <summary>
        /// 获取分类的所有图书关联
        /// </summary>
        /// <param name="categoryId">分类ID</param>
        /// <returns>图书关联列表</returns>
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<BookCategoryDetailDto>>> GetCategoryBooks(string categoryId)
        {
            try
            {
                var books = await _bookCategoryService.GetCategoryBooksAsync(categoryId);
                return Ok(books);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
            }
        }

        /// <summary>
        /// 获取所有叶子节点分类（用于绑定选择）
        /// </summary>
        /// <returns>叶子节点分类列表</returns>
        [HttpGet("leaf-categories")]
        public async Task<ActionResult<IEnumerable<CategorySelectDto>>> GetLeafCategories()
        {
            try
            {
                var categories = await _bookCategoryService.GetLeafCategoriesAsync();
                Console.WriteLine($"返回的叶子分类数量: {categories.Count()}");
                foreach (var category in categories)
                {
                    Console.WriteLine($"分类ID: {category.CategoryID}, 名称: {category.CategoryName}, 路径: {category.CategoryPath}");
                }
                return Ok(categories);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取叶子分类时发生错误: {ex.Message}");
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
            }
        }

        /// <summary>
        /// 获取图书分类关联统计
        /// </summary>
        /// <returns>统计信息</returns>
        [HttpGet("stats")]
        public async Task<ActionResult<Dictionary<string, int>>> GetBookCategoryStats()
        {
            try
            {
                var stats = await _bookCategoryService.GetBookCategoryStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
            }
        }

        /// <summary>
        /// 检查图书分类关联是否存在
        /// </summary>
        /// <param name="isbn">图书ISBN</param>
        /// <param name="categoryId">分类ID</param>
        /// <returns>是否存在</returns>
        [HttpGet("exists/{isbn}/{categoryId}")]
        public async Task<ActionResult<bool>> CheckBookCategoryExists(string isbn, string categoryId)
        {
            try
            {
                var categories = await _bookCategoryService.GetBookCategoriesAsync(isbn);
                var exists = categories.Any(c => c.CategoryID == categoryId);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
            }
        }
    }
}