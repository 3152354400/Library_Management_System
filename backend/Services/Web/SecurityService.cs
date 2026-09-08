using backend.Common.Constants;
using backend.DTOs.Web;
using backend.Models;

namespace backend.Services.Web
{
    public class SecurityService
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public SecurityService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        /**
         * 
         * 从HttpContext中获取登录用户
         */
        public LoginUser GetLoginUser()
        {
            var loginUser = _httpContextAccessor?.HttpContext?.Items["LoginUser"] as LoginUser;
            if (loginUser == null)
            {
                throw new UnauthorizedAccessException("用户未登录或登录信息已过期。");
            }
            return loginUser;
        }

        /**
         * 
         * 从HttpContext中获取登录用户的类型
         */
        public string GetLoginUserType()
        {
            return GetLoginUser()?.UserType ?? string.Empty;
        }

        /**
         * 
         * 从HttpContext中获取登录用户名UserName
         */
        public string GetLoginUserName()
        {
            return GetLoginUser().UserName;
        }

        /**
         * 
         * 从HttpContext中获取登录用户ID
         * 读者返回 ReaderID，管理员返回LibrarianID
         */
        public long GetLoginUserID()
        {
            var loginUser = GetLoginUser();

            var user = loginUser.User;

            long userID = 0;

            if (CheckIsReader(loginUser)) 
            {
                userID = (user as Reader)?.ReaderID ?? 0;
            }else if (CheckIsLibrarian(loginUser))
            {
                userID = (user as Librarian)?.LibrarianID ?? 0;
            }


            return userID;
        }

        /**
         * 
         * 检查登录用户是否为读者
         */
        public bool CheckIsReader(LoginUser loginUser)
        {

            var user = loginUser.User;

            return loginUser.UserType == UserConstants.UserTypeReader || user is Reader;
        }

        /**
         * 
         * 检查当前登录用户是否为读者
         */
        public bool CheckIsReader()
        {
            return CheckIsReader(GetLoginUser());
        }

        /**
 * 
 * 检查登录用户是否为管理员
 */
        public bool CheckIsLibrarian(LoginUser loginUser)
        {

            var user = loginUser.User;

            return loginUser.UserType == UserConstants.UserTypeLibrarian || user is Librarian;
        }

        /**
         * 
         * 检查当前登录用户是否为管理员
         */
        public bool CheckIsLirarian()
        {
            return CheckIsLibrarian(GetLoginUser());
        }

        public Reader? GetReaderFromToken()
        {
            var loginUser = GetLoginUser();
            if (CheckIsReader(loginUser))
            {
                return loginUser.User as Reader;
            }
            return null;
        }

    }
}
