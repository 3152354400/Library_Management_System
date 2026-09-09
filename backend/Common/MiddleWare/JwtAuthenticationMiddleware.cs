namespace backend.Common.MiddleWare
{
    public class JwtAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
 

        public JwtAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // 配置不需要验证的路径，以一下路径为根的所有路径都会放行
        private static readonly string[] _excludedPaths = new[]
        {
        "/api/login",
        "/api/register",          // 登录和注册接口
        "/api/docs",              //swagger UI 的根路径
        "/api/docs/index.html",   //swagger UI 的入口文件
        "/favicon.ico",           // 网站图标
        "/api/Book/search",       // search book
        "/api/bookshelf/search_book_which_shelf",//bookshelf
        "/api/bookshelf/has-books",
        "/api/bookshelf/check-shelf-exists",
        "/api/bookshelf/find-shelf-id",
        "/api/bookshelf/search_bookshelf",
        "/api/bookshelf/return-book",
        "/api/bookshelf/shelf-books",//bookshelf
        "/api/announcements/public",
        "/api/comment/search",
    };


        public async Task Invoke(HttpContext context,TokenService tokenService) {

            var path = context.Request.Path.Value;

            // 先单独放行根路径请求，主要是去掉初始报错
            if (path == "/")
            {
                await _next(context);
                return;
            }

            // 判断是否跳过认证
            if (_excludedPaths.Any(p => path?.StartsWith(p, StringComparison.OrdinalIgnoreCase) == true))
            {
                await _next(context); // 直接放行
                return;
            }

            var loginUser = await tokenService.GetLoginUserAsync(context);
            if (loginUser != null)
            {
                //每次请求都校验token是否过期，如不过期，则刷新token
                await tokenService.VerifyToken(loginUser);

                // 将登录用户信息存入HttpContext
                context.Items["LoginUser"] = loginUser;
            }
            else
            {

                throw new UnauthorizedAccessException("未认证，请先登录。");
            }
            await _next(context); // 调用下一个中间件
        }
    }
}
