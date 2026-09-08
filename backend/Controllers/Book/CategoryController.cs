using backend.DTOs.Book;
using backend.Repositories.Book;
using backend.Services.Web;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Book
{
    /// <summary>
    /// 分类管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly BookCategoryTreeOperation _categoryTreeOperation;
        private readonly SecurityService _securityService;

        public CategoryController(BookCategoryTreeOperation categoryTreeOperation, SecurityService securityService)
        {
            _categoryTreeOperation = categoryTreeOperation;
            _securityService = securityService;
        }

        /// <summary>
        /// 获取分类树
        /// </summary>
        /// <returns>分类树结构</returns>
        [HttpGet("tree")]
        public async Task<ActionResult<IEnumerable<CategoryNode>>> GetCategoryTree()
        {
            try
            {
                var categories = await _categoryTreeOperation.GetAllCategoriesAsync();
                var categoryDict = categories.ToDictionary(c => c.CategoryID, c => new CategoryNode
                {
                    CategoryID = c.CategoryID,
                    CategoryName = c.CategoryName,
                    ParentCategoryID = c.ParentCategoryID,
                    Children = new List<CategoryNode>()
                });

                var rootNodes = new List<CategoryNode>();

                foreach (var category in categoryDict.Values)
                {
                    if (string.IsNullOrEmpty(category.ParentCategoryID))
                    {
                        rootNodes.Add(category);
                    }
                    else if (categoryDict.ContainsKey(category.ParentCategoryID))
                    {
                        categoryDict[category.ParentCategoryID].Children.Add(category);
                    }
                }

                return Ok(rootNodes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
            }
        }

        /// <summary>
        /// 添加分类
        /// </summary>
        /// <param name="request">分类请求</param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public async Task<ActionResult> AddCategory([FromBody] CategoryRequest request)
        {
            try
            {
                // 添加调试日志
                Console.WriteLine($"收到添加分类请求: {System.Text.Json.JsonSerializer.Serialize(request)}");
                
                if (request == null)
                {
                    return BadRequest(new { message = "请求数据不能为空" });
                }
                
                if (request.Category == null)
                {
                    return BadRequest(new { message = "分类数据不能为空" });
                }
                
                var category = request.Category;
                
                // 验证分类名称不能为空
                if (string.IsNullOrWhiteSpace(category.CategoryName))
                {
                    return BadRequest(new { message = "分类名称不能为空" });
                }

                // 验证分类ID不能为空
                if (string.IsNullOrWhiteSpace(category.CategoryID))
                {
                    return BadRequest(new { message = "分类ID不能为空" });
                }

                // 检查分类ID是否已存在
                var existingCategory = await _categoryTreeOperation.GetCategoryByIdAsync(category.CategoryID);
                if (existingCategory != null)
                {
                    return BadRequest(new { message = "分类ID已存在" });
                }

                // 检查分类名称在同级中是否重复
                var isDuplicate = await _categoryTreeOperation.IsCategoryNameDuplicateAsync(
                    category.CategoryName, 
                    category.ParentCategoryID
                );
                if (isDuplicate)
                {
                    return BadRequest(new { message = "同级分类中已存在相同名称" });
                }

                // 如果有父分类，验证父分类是否存在
                if (!string.IsNullOrEmpty(category.ParentCategoryID))
                {
                    var parentCategory = await _categoryTreeOperation.GetCategoryByIdAsync(category.ParentCategoryID);
                    if (parentCategory == null)
                    {
                        return BadRequest(new { message = "父分类不存在" });
                    }
                }

                // 添加分类
                var result = await _categoryTreeOperation.AddCategoryAsync(category);
                if (result > 0)
                {
                    return Ok(new { message = "分类添加成功" });
                }
                else
                {
                    return BadRequest(new { message = "分类添加失败" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"添加分类时发生错误: {ex.Message}");
                Console.WriteLine($"错误堆栈: {ex.StackTrace}");
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// 更新分类
        /// </summary>
        /// <param name="request">分类请求</param>
        /// <returns>操作结果</returns>
        [HttpPut]
        public async Task<ActionResult> UpdateCategory([FromBody] CategoryRequest request)
        {
            try
            {
                var category = request.Category;
                
                // 验证分类名称不能为空
                if (string.IsNullOrWhiteSpace(category.CategoryName))
                {
                    return BadRequest(new { message = "分类名称不能为空" });
                }

                // 验证分类ID不能为空
                if (string.IsNullOrWhiteSpace(category.CategoryID))
                {
                    return BadRequest(new { message = "分类ID不能为空" });
                }

                // 检查分类是否存在
                var existingCategory = await _categoryTreeOperation.GetCategoryByIdAsync(category.CategoryID);
                if (existingCategory == null)
                {
                    return BadRequest(new { message = "分类不存在" });
                }

                // 检查分类名称在同级中是否重复（排除自己）
                var isDuplicate = await _categoryTreeOperation.IsCategoryNameDuplicateAsync(
                    category.CategoryName, 
                    category.ParentCategoryID,
                    category.CategoryID
                );
                if (isDuplicate)
                {
                    return BadRequest(new { message = "同级分类中已存在相同名称" });
                }

                // 如果有父分类，验证父分类是否存在且不能是自己
                if (!string.IsNullOrEmpty(category.ParentCategoryID))
                {
                    if (category.ParentCategoryID == category.CategoryID)
                    {
                        return BadRequest(new { message = "分类不能将自己设为父分类" });
                    }

                    var parentCategory = await _categoryTreeOperation.GetCategoryByIdAsync(category.ParentCategoryID);
                    if (parentCategory == null)
                    {
                        return BadRequest(new { message = "父分类不存在" });
                    }
                }

                // 更新分类
                var result = await _categoryTreeOperation.UpdateCategoryAsync(category);
                if (result > 0)
                {
                    return Ok(new { message = "分类更新成功" });
                }
                else
                {
                    return BadRequest(new { message = "分类更新失败" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
            }
        }

        /// <summary>
        /// 删除分类
        /// </summary>
        /// <param name="id">分类ID</param>
        /// <param name="operatorId">操作员ID</param>
        /// <returns>操作结果</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCategory(string id, [FromQuery] string operatorId)
        {
            try
            {
                // 检查分类是否存在
                var category = await _categoryTreeOperation.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return BadRequest(new { message = "分类不存在" });
                }

                // 检查分类下是否有关联图书
                var hasBooks = await _categoryTreeOperation.HasBookInCategoryAsync(id);
                if (hasBooks)
                {
                    return BadRequest(new { message = "该分类下还有关联图书，无法删除" });
                }

                // 检查分类下是否有子分类
                var childCount = await _categoryTreeOperation.GetChildCategoryCountAsync(id);
                if (childCount > 0)
                {
                    return BadRequest(new { message = "该分类下还有子分类，无法删除" });
                }

                // 删除分类
                var result = await _categoryTreeOperation.DeleteCategoryAsync(id);
                if (result > 0)
                {
                    return Ok(new { message = "分类删除成功" });
                }
                else
                {
                    return BadRequest(new { message = "分类删除失败" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
            }
        }

        /// <summary>
        /// 根据ID获取分类
        /// </summary>
        /// <param name="id">分类ID</param>
        /// <returns>分类信息</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(string id)
        {
            try
            {
                var category = await _categoryTreeOperation.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new { message = "分类不存在" });
                }

                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
            }
        }
    }
}
