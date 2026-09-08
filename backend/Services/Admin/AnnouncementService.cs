using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTOs.Admin;
using backend.Repositories.Admin;

namespace backend.Services.Admin
{
    public class AnnouncementService
    {
        private readonly AnnouncementRepository _repository;

        public AnnouncementService(AnnouncementRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<AnnouncementDto>> GetAllAnnouncementsAsync()
        {
            return _repository.GetAllAnnouncementsAsync();
        }

        public async Task<PublicAnnouncementsDto> GetPublicAnnouncementsAsync()
        {
            var allPublic = await _repository.GetPublicAnnouncementsAsync();

            return new PublicAnnouncementsDto
            {
                Urgent = Enumerable.Empty<AnnouncementDto>(),
                Regular = allPublic.Take(3)
            };
        }

        public Task<AnnouncementDto> CreateAnnouncementAsync(UpsertAnnouncementDto dto, int librarianId)
        {
            return _repository.CreateAnnouncementAsync(dto, librarianId);
        }

        public async Task<AnnouncementDto> UpdateAnnouncementAsync(int id, UpsertAnnouncementDto dto)
        {
            var announcement = await _repository.UpdateAnnouncementAsync(id, dto);
            return announcement ?? throw new KeyNotFoundException("公告不存在");
        }

        public Task<bool> TakedownAnnouncementAsync(int id)
        {
            return _repository.UpdateStatusAsync(id, "已撤回");
        }
    }
}