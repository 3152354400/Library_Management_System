using backend.Common.Constants;
using backend.Common.Utils;
using backend.DTOs.Reader;
using backend.Models;
using backend.Services.ReaderService;
using backend.Services.RecommendationService;
using backend.Services.Web;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReaderController : ControllerBase
    {
        private readonly ReaderService _readerService;

        private readonly SecurityService _securityService;

        private readonly TokenService _tokenService;

        private readonly RecommendationService _recommendationService;

        /**
         * 构造函数
         * @param readerService Reader 服务依赖
         * @return 无
         */
        public ReaderController(ReaderService readerService, SecurityService securityService, TokenService tokenService, RecommendationService recommendationService)
        {
            _readerService = readerService;
            _securityService = securityService;
            _tokenService = tokenService;
            _recommendationService = recommendationService;
        }

        /**
         * 获取所有 Reader
         * @return Reader 列表
         */
        [HttpGet("list")]
        public async Task<ActionResult> list(string? username = null)
        {
            var readers = await _readerService.GetAllReadersAsync(username);
            return Ok(readers);
        }

        /**
         * 根据 ID 获取 Reader
         * @param id ReaderID
         * @return Reader 对象
         */
        [HttpGet("{id}")]
        public async Task<ActionResult> GetReader(long id)
        {
            var reader = await _readerService.GetReaderByReaderIDAsync(id);
            if (reader == null) return NotFound();
            return Ok(reader);
        }

        /**
         * 添加一个 Reader
         * @param dto ReaderDto
         * @return 结果状态
         */
        [HttpPost]
        public async Task<ActionResult> Add([FromBody] Reader reader)
        {

            var result = await _readerService.InsertReaderAsync(reader);
            return result > 0 ? Ok() : BadRequest();
        }

        /**
         * 更新一个 Reader
         * @param dto ReaderDto
         * @return 结果状态
         */
        [HttpPut]
        public async Task<ActionResult> Update([FromBody] Reader reader)
        {

            var result = await _readerService.UpdateReaderAsync(reader);
            return result > 0 ? Ok() : NotFound();
        }

        [HttpPut("updatePartial")]
        public async Task<ActionResult> UpdatePartial([FromBody] ReaderShowDto reader)
        {
            var result = await _readerService.UpdateReaderPartialAsync(reader);
            return result > 0 ? Ok() : NotFound();
        }

        /**
         * 删除一个 Reader
         * @param id ReaderID
         * @return 结果状态
         */
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            var result = await _readerService.DeleteReaderAsync(id);
            return result > 0 ? Ok() : NotFound();
        }

        /**
         * 重置密码
         * @
         * 
         */
        [HttpPut("me/resetPwd")]
        public async Task<ActionResult> ResetPassword(string OldPwd, string NewPwd)
        {
            var loginUser = _securityService.GetLoginUser();
            string userName = loginUser.UserName;
            string password = loginUser.Password;

            if (PasswordUtils.VerifyPassword(OldPwd, password))
            {
                return BadRequest("修改密码失败，旧密码不正确");
            }
            else if (PasswordUtils.VerifyPassword(NewPwd, password))
            {
                return BadRequest("修改密码失败，新密码不能和旧密码相同");
            }

            NewPwd = PasswordUtils.HashPassword(NewPwd); // 确保新密码被哈希处理

            if (await _readerService.ResetPasswordAsync(userName, NewPwd))
            {
                loginUser.Password = NewPwd; // 更新登录用户的密码
                await _tokenService.SetLoginUserAsync(loginUser);
                return Ok("密码重置成功");
            }
            return BadRequest("密码重置失败");
        }


        /**
         * 重置密码
         * @
         * 
         */
        [HttpPut("resetPwd")]
        public async Task<ActionResult> Reset(string userName, string NewPwd = UserConstants.DefaultPassword)
        {

            NewPwd = PasswordUtils.HashPassword(NewPwd); // 确保新密码被哈希处理

            if (await _readerService.ResetPasswordAsync(userName, NewPwd))
            {
                return Ok("密码重置成功");
            }
            return BadRequest("密码重置失败");
        }

        /**
         * 获取登录用户信息
         * @return 登录用户信息
         */
        [HttpGet("me/info")]
        public ActionResult Info()
        {

            var loginUser = _securityService.GetLoginUser();

            // 检查登录用户是否为 Reader
            if (_securityService.CheckIsReader(loginUser))
            {
                var reader = loginUser.User as Reader;
                var avatarUrl = reader?.Avatar;

                if (avatarUrl == UserConstants.AvatarUrlNull || avatarUrl == UserConstants.AvatarUrlEmpty || avatarUrl == UserConstants.AvatarUrlDefault)
                {
                    if (reader != null)
                        reader.Avatar = UserConstants.SystemAvatar0;
                }

                var readerDetail = new ReaderDetailDto
                {
                    UserName = reader?.UserName ?? string.Empty,
                    FullName = reader?.FullName,
                    NickName = reader?.NickName,
                    Avatar = reader?.Avatar,
                    CreditScore = reader?.CreditScore ?? 0,
                    AccountStatus = reader?.AccountStatus ?? string.Empty,
                    Permission = reader?.Permission ?? string.Empty
                };
                return Ok(readerDetail);
            }
            //todo

            return BadRequest("获取详细信息错误");
        }


        /**
         * 
         * 获取头像
         */
        [HttpGet("me/avatar/{filename}")]
        public ActionResult GetAvatar(string filename)
        {
            // 防止路径穿越攻击（如 filename = "../../appsettings.json"）
            if (filename.Contains("..") || Path.IsPathRooted(filename))
                return BadRequest("非法文件名");

            //var avatarDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
            var filePath = Path.Combine(UserConstants.AvatarDirectoryRoot, filename);

            if (!System.IO.File.Exists(filePath))
            {
                filePath = Path.Combine(UserConstants.AvatarDirectoryRoot, UserConstants.SystemAvatar0); // 默认头像
            }

            // 自动识别图片类型（推荐使用 content-type 识别器）
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var contentType = extension switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                _ => "application/octet-stream" // 默认未知类型
            };

            return PhysicalFile(filePath, contentType);
        }

        /**
         * 
         * 上传头像文件到服务器
         */
        [HttpPost("me/upload/avatar")]
        public async Task<ActionResult> UploadAvatar(IFormFile file)
        {
            // 检查文件是否存在
            if (file == null || file.Length == 0)
                return BadRequest("请上传头像文件");

            var allowedTypes = new[] { ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowedTypes.Contains(ext))
                return BadRequest("仅支持图片格式（jpg/jpeg/png）");

            if (file.Length > UserConstants.FileSize1MB) // 1MB 限制
                return BadRequest("文件大小不能超过 1MB");

            var loginUser = _securityService.GetLoginUser();
            if (_securityService.CheckIsReader(loginUser))
            {
                var reader = loginUser.User as Reader;

                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);//用uuid生成文件名
                var filePath = Path.Combine(UserConstants.AvatarDirectoryRoot, fileName);

                // 删除旧头像（排除以 System 开头的）
                if (!string.IsNullOrEmpty(reader?.Avatar) && !reader.Avatar.StartsWith("system"))
                {
                    System.IO.File.Delete(Path.Combine(UserConstants.AvatarDirectoryRoot, reader.Avatar));
                }

                //保存新头像到服务器
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var url = fileName; // 头像访问地址

                // 更新 Reader 的头像 URL
                if (reader != null)
                    reader.Avatar = url;

                //更新登录用户信息
                await _tokenService.SetLoginUserAsync(loginUser);

                //返回头像 URL
                return await _readerService.UpdateAvatarAsync(reader?.ReaderID ?? 0, url) ? Ok(url) : BadRequest("更新头像失败");
            }
            return BadRequest("上传头像失败");
        }

        /**
         * 更新头像URL
         * 
         */
        [HttpPut("me/avatar")]
        public async Task<ActionResult> UpdateAvatar([FromQuery] string avatarUrl)
        {
            var loginUser = _securityService.GetLoginUser();
            if (_securityService.CheckIsReader(loginUser))
            {
                var reader = loginUser.User as Reader;
                // 验证 URL 是否有效
                if (string.IsNullOrEmpty(avatarUrl) || !Uri.IsWellFormedUriString(avatarUrl, UriKind.RelativeOrAbsolute))
                {
                    return BadRequest("无效的头像 URL");
                }

                // 删除旧头像（排除以 System 开头的）
                if (!string.IsNullOrEmpty(reader?.Avatar) && !reader.Avatar.StartsWith("system"))
                {
                    System.IO.File.Delete(Path.Combine(UserConstants.AvatarDirectoryRoot, reader.Avatar));
                }

                // 更新 Reader 的头像 URL
                if (reader != null)
                    reader.Avatar = avatarUrl;

                //更新登录用户信息
                await _tokenService.SetLoginUserAsync(loginUser);


                return await _readerService.UpdateAvatarAsync(reader?.ReaderID ?? 0, avatarUrl) ? Ok("头像更新成功") : BadRequest("更新头像失败");
            }

            return BadRequest("更新头像时的未知错误");
        }

        /**
         * 
         * 更新个人信息
         */
        [HttpPut("me/info")]
        public async Task<ActionResult> UpdateInfo([FromBody] ReaderDetailDto readerDetail)
        {
            var loginUser = _securityService.GetLoginUser();
            if (_securityService.CheckIsReader(loginUser))
            {
                var reader = loginUser.User as Reader;

                if (!string.IsNullOrWhiteSpace(readerDetail.UserName))
                {
                    // 检查用户名是否已存在
                    if (reader?.UserName != readerDetail.UserName && await _readerService.IsUserNameExistsAsync(readerDetail.UserName))
                    {
                        return BadRequest("用户名已存在，请选择其他用户名");
                    }
                    if (reader != null)
                        reader.UserName = readerDetail.UserName;
                }

                // 只更新非空非空字符串字段
                if (!string.IsNullOrWhiteSpace(readerDetail.FullName) && reader != null)
                    reader.FullName = readerDetail.FullName;

                if (!string.IsNullOrWhiteSpace(readerDetail.NickName) && reader != null)
                    reader.NickName = readerDetail.NickName;

                // 更新登录用户信息
                await _tokenService.SetLoginUserAsync(loginUser);

                return await _readerService.UpdateProfileAsync(reader!)
                    ? Ok("个人信息更新成功")
                    : BadRequest("更新个人信息失败");
            }
            return BadRequest("更新个人信息时的未知错误");
        }


        /*
         * 
         * 获取推荐图书
         */
        [HttpGet("me/recommendations")]
        public async Task<ActionResult> GetRecommendations()
        {
            var loginUser = _securityService.GetLoginUser();
            if (_securityService.CheckIsReader(loginUser))
            {
                var reader = loginUser.User as Reader;
                if (reader == null)
                    return BadRequest("获取推荐图书失败，读者信息无效");
                var recommendations = await _recommendationService.GetRecommendationsAsync(reader.ReaderID);
                return Ok(recommendations);
            }
            return BadRequest("获取推荐图书失败，用户信息无效");
        }
    }
}