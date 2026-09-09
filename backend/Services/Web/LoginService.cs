using backend.Common.Constants;
using backend.Common.Utils;
using backend.DTOs.Web;
using backend.Models;
using backend.Repositories.LibrarianRepository;
using backend.Repositories.ReaderRepository;

namespace backend.Services.Web
{
    public class LoginService
    {
        private readonly ReaderRepository _readerRepository;
        private readonly TokenService _tokenService;
        private readonly SecurityService _securityService;
        private readonly LibrarianRepository _librarianRepository;

        public LoginService(ReaderRepository readerRepository, TokenService tokenService, SecurityService securityService, LibrarianRepository librarianRepository)
        {
            _readerRepository = readerRepository;
            _tokenService = tokenService;
            _securityService = securityService;
            _librarianRepository = librarianRepository;
        }

        public async Task<string> LoginAsync(LoginDto loginDto)
        {
            string userName = loginDto.UserName;
            string password = loginDto.Password;
            string userType = loginDto.UserType;

            //验证码校验
            ValidateCaptha();
            //前置校验，检查用户名和密码是否合法（为空或长度不符合要求）
            PreCheck(userName, password);

            User? user = null;

            if(userType == UserConstants.UserTypeLibrarian)
            {
                user = _librarianRepository.GetByStaffNoAsync(userName).Result;
            }
            else if (userType == UserConstants.UserTypeReader)
            {
                user = _readerRepository.GetByUserNameAsync(userName).Result;
            }
            else
            {
                throw new InvalidOperationException("无效用户类型");
            }


            if (user == null)
            {
                throw new KeyNotFoundException("账号或密码错误");//和密码错误使用同一个状态码
            }

            //检查密码是否匹配
            if (!PasswordUtils.VerifyPassword(password, user.Password))//密码不匹配
            {
                throw new KeyNotFoundException("用户名或密码错误");//和用户名不存在使用同一个状态码
            }

            //创建LoginUser对象
            LoginUser loginUser = new LoginUser(user,userType);

            //生成token并存入到Redis中
            return await _tokenService.CreateTokenAsync(loginUser);
        }

        //验证码校验
        private void ValidateCaptha()
        {

        }

        //前置校验，检查用户名和密码是否合法（为空或长度不符合要求）
        private void PreCheck(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("用户名或密码不能为空");
            }
            if (userName.Length < UserConstants.UsernameMinLength || userName.Length > UserConstants.UsernameMaxLength)
            {
                throw new ArgumentException("用户名长度必须在2到20个字符之间");
            }
            if (password.Length < UserConstants.PasswordMinLength || password.Length > UserConstants.PasswordMaxLength)
            {
                throw new ArgumentException("密码长度必须在5到20个字符之间");
            }
        }

        //退出登录
        public async Task<bool> LogoutAsync()
        {
            var loginUser = _securityService.GetLoginUser();
            if (loginUser == null || string.IsNullOrEmpty(loginUser.Token))
            {
                throw new UnauthorizedAccessException("未认证，请先登录。");
            }
            //删除Redis中的token
            var result = await _tokenService.DeleteTokenAsync(loginUser.Token);
            if (!result)
            {
                throw new InvalidOperationException("退出登录失败，请稍后再试。");
            }
            return result;
        }

    }
}

