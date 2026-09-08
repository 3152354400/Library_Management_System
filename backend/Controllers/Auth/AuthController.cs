using backend.DTOs.Reader;
using backend.DTOs.Web;
using backend.Services.ReaderService;
using backend.Services.Web;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.AuthController
{

    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly LoginService _loginService;
        private readonly ReaderService _readerService;

        public AuthController(LoginService loginService, ReaderService readerService)
        {
            _loginService = loginService;
            _readerService = readerService;
        }

        //登录
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        {
            return Ok(await _loginService.LoginAsync(loginDto));
        }

        //注册
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] ReaderRegisterDto registerDto)
        {
            return await _readerService.RegisterReaderAsync(registerDto) ? Ok("注册成功") : BadRequest("注册失败");
        }

        //退出登录
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {

            return await _loginService.LogoutAsync() ? Ok("已成功退出登录") : BadRequest("退出失败");
        }
    }
}
