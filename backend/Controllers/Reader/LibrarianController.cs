using backend.Models;
using backend.Services.LibrarianService;
using backend.Services.Web;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LibrarianController : ControllerBase
    {
       private readonly LibrarianService _librarianService;

        private readonly SecurityService _securityService;



        /**
         * 构造函数
         * @param librarianService Librarian 服务依赖
         * @return 无
         */
        public LibrarianController(LibrarianService librarianService, SecurityService securityService)
        {
            _librarianService = librarianService;
            _securityService = securityService;
        }

        /**
         * 获取所有 Librarian
         * @return Librarian 列表
         */
        [HttpGet("list")]
        public async Task<ActionResult> list()
        {
            var librarians = await _librarianService.GetAllLibrariansAsync();
            return Ok(librarians);
        }

        /**
         * 根据 ID 获取 Librarian
         * @param id LibrarianID
         * @return Librarian 对象
         */
        [HttpGet("{id}")]
        public async Task<ActionResult> GetLibrarian(long id)
        {
            var librarian = await _librarianService.GetLibrarianByLibrarianIDAsync(id);
            if (librarian == null) return NotFound();
            return Ok(librarian);
        }

        /**
         * 添加一个 Librarian
         * @param dto LibrarianDto
         * @return 结果状态
         */
        [HttpPost]
        public async Task<ActionResult> Add([FromBody] Librarian librarian)
        {

            var result = await _librarianService.InsertLibrarianAsync(librarian);
            return result > 0 ? Ok() : BadRequest();
        }

        /**
         * 更新一个 Librarian
         * @param dto LibrarianDto
         * @return 结果状态
         */
        [HttpPut]
        public async Task<ActionResult> Update([FromBody] Librarian librarian)
        {

            var result = await _librarianService.UpdateLibrarianAsync(librarian);
            return result > 0 ? Ok() : NotFound();
        }

        /**
         * 删除一个 Librarian
         * @param id LibrarianID
         * @return 结果状态
         */
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            var result = await _librarianService.DeleteLibrarianAsync(id);
            return result > 0 ? Ok() : NotFound();
        }

        /**
         * 获取登录用户信息
         * @return 登录用户信息
         */
        [HttpGet("info")]
        public ActionResult Info()
        {

            var loginUser = _securityService.GetLoginUser();

            // 检查登录用户是否为 Librarian
            if (_securityService.CheckIsLibrarian(loginUser))
            {
                var librarian = loginUser.User as Librarian;

                return Ok(librarian);
            }

            return BadRequest("当前登录用户类型错误");
        }

    }
}
