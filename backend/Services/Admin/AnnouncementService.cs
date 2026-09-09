using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTOs.Admin;
using backend.Repositories.Admin;
using backend.Services.Web;

namespace backend.Services.Admin
{
    public class AnnouncementService
    {
        private readonly AnnouncementRepository _repository;
        private readonly RedisService _redis;

        // 缓存配置
        private const string PublicAnnouncementsCacheKey = "announcements:public";
        private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5); // 缓存5分钟

        public AnnouncementService(AnnouncementRepository repository, RedisService redis)
        {
            _repository = repository;
            _redis = redis;
        }

        public async Task<IEnumerable<AnnouncementDto>> GetAllAnnouncementsAsync()
        {
            return await _repository.GetAllAnnouncementsAsync();
        }

        public async Task<PublicAnnouncementsDto> GetPublicAnnouncementsAsync()
        {
            // 尝试从缓存获取（Redis挂了不影响主流程）
            try
            {
                var cached = await _redis.GetCacheAsync<PublicAnnouncementsDto>(PublicAnnouncementsCacheKey);
                if (cached != null)
                {
                    return cached;
                }
            }
            catch (Exception ex)
            {
                // 缓存异常，降级到数据库查询
                Console.WriteLine($"[Redis缓存读取失败] {ex.Message}");
            }

            // 从数据库获取
            var allPublic = await _repository.GetPublicAnnouncementsAsync();

            var result = new PublicAnnouncementsDto
            {
                Urgent = Enumerable.Empty<AnnouncementDto>(),
                Regular = allPublic.Take(3)
            };

            // 存入缓存（缓存失败不影响返回结果）
            try
            {
                await _redis.SetCacheAsync(PublicAnnouncementsCacheKey, result, CacheExpiry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis缓存写入失败] {ex.Message}");
            }

            return result;
        }

        public async Task<AnnouncementDto> CreateAnnouncementAsync(UpsertAnnouncementDto dto, int librarianId)
        {
            var result = await _repository.CreateAnnouncementAsync(dto, librarianId);

            // 创建公告后清除缓存（失败不影响主流程）
            try { await _redis.DeleteAsync(PublicAnnouncementsCacheKey); }
            catch (Exception ex) { Console.WriteLine($"[Redis缓存清除失败] {ex.Message}"); }

            return result;
        }

        public async Task<AnnouncementDto> UpdateAnnouncementAsync(int id, UpsertAnnouncementDto dto)
        {
            var announcement = await _repository.UpdateAnnouncementAsync(id, dto);
            if (announcement == null) throw new KeyNotFoundException("公告不存在");

            // 更新公告后清除缓存
            try { await _redis.DeleteAsync(PublicAnnouncementsCacheKey); }
            catch (Exception ex) { Console.WriteLine($"[Redis缓存清除失败] {ex.Message}"); }

            return announcement;
        }

        public async Task<bool> TakedownAnnouncementAsync(int id)
        {
            var result = await _repository.UpdateStatusAsync(id, "已撤回");

            // 撤回公告后清除缓存
            if (result)
            {
                try { await _redis.DeleteAsync(PublicAnnouncementsCacheKey); }
                catch (Exception ex) { Console.WriteLine($"[Redis缓存清除失败] {ex.Message}"); }
            }

            return result;
        }
    }
}