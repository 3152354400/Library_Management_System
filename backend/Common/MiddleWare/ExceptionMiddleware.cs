using System.Text.Json;

namespace backend.Common.MiddleWare
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // 继续下一个中间件
            }
            catch (Exception ex)
            {

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";


            context.Response.StatusCode = exception switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                FormatException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status400BadRequest,

                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                System.Security.SecurityException => StatusCodes.Status403Forbidden,

                KeyNotFoundException => StatusCodes.Status404NotFound,
                NotSupportedException => StatusCodes.Status405MethodNotAllowed,

                TimeoutException => StatusCodes.Status408RequestTimeout,
                OperationCanceledException => 499, // Nginx 自定义，或你自己定义

                IOException => StatusCodes.Status503ServiceUnavailable,
                NullReferenceException => StatusCodes.Status500InternalServerError,

                //可以写一些自定义异常并且对应异常码

                //兜底异常
                _ => StatusCodes.Status500InternalServerError
            };


            // 将异常信息写入响应体
            await context.Response.WriteAsync(JsonSerializer.Serialize(exception.Message));

        }

    }
}
