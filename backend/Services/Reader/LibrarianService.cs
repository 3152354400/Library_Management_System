using backend.Models;
using backend.Repositories.LibrarianRepository;

namespace backend.Services.LibrarianService
{
    public class LibrarianService
    {

        private readonly LibrarianRepository _librarianRepository;

        /**
         * 锟斤拷锟届函锟斤拷
         * @param librarianRepository Librarian 锟街达拷锟斤拷锟斤拷
         * @return 锟斤拷
         */
        public LibrarianService(LibrarianRepository librarianRepository)
        {
            _librarianRepository = librarianRepository;
        }

        /**
         * 锟斤拷锟斤拷 LibrarianID 锟斤拷取 Librarian 锟斤拷锟斤拷
         * @param librarianID 锟斤拷锟斤拷 ID
         * @return Librarian 锟斤拷锟斤拷锟?null
         */
        public async Task<Librarian> GetLibrarianByLibrarianIDAsync(long librarianID)
        {
            return await _librarianRepository.GetByLibrarianIDAsync(librarianID);
        }

        /**
         * 根据 StaffNo 获取 Librarian 对象
         * @param StaffNo 用户名,通常是学号
         * @return Librarian 对象或 null
         */
        public async Task<Librarian> GetLibrarianByStaffNoAsync(string staffNo)
        {
            return await _librarianRepository.GetByStaffNoAsync(staffNo);
        }

        /**
         * 锟斤拷取锟斤拷锟斤拷 Librarian 锟斤拷锟斤拷
         * @return Librarian 锟斤拷锟斤拷锟叫憋拷
         */
        public async Task<IEnumerable<Librarian>> GetAllLibrariansAsync()
        {
            return await _librarianRepository.GetAllLibrariansAsync();
        }

        /**
         * 锟斤拷锟斤拷一锟斤拷锟铰碉拷 Librarian
         * @param librarian Librarian 实锟斤拷
         * @return 锟斤拷影锟斤拷锟斤拷锟斤拷锟?
         */
        public async Task<int> InsertLibrarianAsync(Librarian librarian)
        {
            return await _librarianRepository.InsertLibrarianAsync(librarian);
        }

        /**
         * 锟斤拷锟斤拷一锟斤拷 Librarian
         * @param librarian Librarian 实锟斤拷
         * @return 锟斤拷影锟斤拷锟斤拷锟斤拷锟?
         */
        public async Task<int> UpdateLibrarianAsync(Librarian librarian)
        {
            return await _librarianRepository.UpdateLibrarianAsync(librarian);
        }

        /**
         * 删锟斤拷一锟斤拷 Librarian
         * @param librarianID LibrarianID
         * @return 锟斤拷影锟斤拷锟斤拷锟斤拷锟?
         */
        public async Task<int> DeleteLibrarianAsync(long librarianID)
        {
            return await _librarianRepository.DeleteLibrarianAsync(librarianID);
        }

        /**
       * 检查用户名是否已存在
       * @param staffNo 用户名
       * @return true 如果用户名已存在，否则 false
       */
        public async Task<bool> IsStaffNoExistsAsync(string staffNo)
        {
            return await _librarianRepository.IsStaffNoExistsAsync(staffNo);
        }
    }
}
