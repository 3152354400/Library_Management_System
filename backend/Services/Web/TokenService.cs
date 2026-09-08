using backend.DTOs.Web;
using backend.Services.Web;

public class TokenService
{
    private readonly IConfiguration _config;
    private readonly RedisService _redisService;
    private readonly int _expireMinutes;

    public TokenService(IConfiguration config, RedisService redisService)
    {
        _config = config;
        _redisService = redisService;
        _expireMinutes = _config.GetValue<int>("Jwt:ExpireMinutes", 30); // 默认30分钟
    }

    public async Task<LoginUser?> GetLoginUserAsync(HttpContext httpContext)
    {

        var token = httpContext.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(token))
        {
            if (token.Trim().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Trim().Substring("Bearer ".Length).Trim();
            }

            return await GetLoginUserAsync(token);
        }
        return null;
    }

    public async Task<string> CreateTokenAsync(LoginUser loginUser)
    {
        var token = JwtUtils.CreateToken(loginUser, _config);
        loginUser.Token = token;
        await RefreshTokenAsync(loginUser);
        return token;
    }

    public async Task<LoginUser?> GetLoginUserAsync(string token)
    {
        var redisKey = $"LOGIN_TOKEN:{token}";
        return await _redisService.GetCacheAsync<LoginUser>(redisKey);
    }

    public async Task<bool> DeleteTokenAsync(string token)
    {
        var redisKey = $"LOGIN_TOKEN:{token}";
        return await _redisService.DeleteAsync(redisKey);
    }

    public async Task RefreshTokenAsync(LoginUser loginUser)
    {
        loginUser.LoginTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        loginUser.ExpireTime = loginUser.LoginTime + _expireMinutes * 60;
        await _redisService.SetCacheAsync($"LOGIN_TOKEN:{loginUser.Token}", loginUser, TimeSpan.FromMinutes(_expireMinutes));
    }

    /**
 * 验证令牌有效期，相差不足30分钟，自动刷新缓存
 * 
 * @param loginUser 登录信息
 * @return 令牌
 */
    public async Task VerifyToken(LoginUser loginUser)
    {
        long expireTime = loginUser.ExpireTime;
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();


        if (expireTime - currentTime <= _expireMinutes * 60)
        {
            await RefreshTokenAsync(loginUser);
        }
        await Task.CompletedTask; // 确保方法返回 Task
    }

    /**
     * 
     * 设置用户身份信息
     */
    public async Task SetLoginUserAsync(LoginUser loginUser)
    {
        if(loginUser == null)
        {
            throw new ArgumentException("登录用户不能为空");
        }
        await RefreshTokenAsync(loginUser);
    }

}
