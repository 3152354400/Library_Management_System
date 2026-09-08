using backend.Common.Constants;
using backend.DTOs.Reader;
using backend.Models;
using backend.Repositories.ReaderRepository;

namespace backend.Services.ReaderService
{
    /**
     * ReaderService 锟结供锟斤拷 ReaderRepository 锟斤拷业锟斤拷锟竭硷拷锟斤拷装
     */
    public class ReaderService
    {
        private readonly ReaderRepository _readerRepository;

        /**
         * 锟斤拷锟届函锟斤拷
         * @param readerRepository Reader 锟街达拷锟斤拷锟斤拷
         * @return 锟斤拷
         */
        public ReaderService(ReaderRepository readerRepository)
        {
            _readerRepository = readerRepository;
        }

        /**
         * 锟斤拷锟斤拷 ReaderID 锟斤拷取 Reader 锟斤拷锟斤拷
         * @param readerID 锟斤拷锟斤拷 ID
         * @return Reader 锟斤拷锟斤拷锟?null
         */
        public async Task<Reader> GetReaderByReaderIDAsync(long readerID)
        {
            return await _readerRepository.GetByReaderIDAsync(readerID);
        }

        /**
         * 根据 UserName 获取 Reader 对象
         * @param UserName 用户名,通常是学号
         * @return Reader 对象或 null
         */
        public async Task<Reader> GetReaderByUserNameAsync(string userName)
        {
            return await _readerRepository.GetByUserNameAsync(userName);
        }

        /**
         * 锟斤拷取锟斤拷锟斤拷 Reader 锟斤拷锟斤拷
         * @return Reader 锟斤拷锟斤拷锟叫憋拷
         */
        public async Task<IEnumerable<Reader>> GetAllReadersAsync(string? username = null)
        {
            return await _readerRepository.GetAllReadersAsync(username);
        }

        /**
         * 锟斤拷锟斤拷一锟斤拷锟铰碉拷 Reader
         * @param reader Reader 实锟斤拷
         * @return 锟斤拷影锟斤拷锟斤拷锟斤拷锟?
         */
        public async Task<int> InsertReaderAsync(Reader reader)
        {
            return await _readerRepository.InsertReaderAsync(reader);
        }

        /**
         * 锟斤拷锟斤拷一锟斤拷 Reader
         * @param reader Reader 实锟斤拷
         * @return 锟斤拷影锟斤拷锟斤拷锟斤拷锟?
         */
        public async Task<int> UpdateReaderAsync(Reader reader)
        {
            return await _readerRepository.UpdateReaderAsync(reader);
        }

        public async Task<int> UpdateReaderPartialAsync(ReaderShowDto dto)
        {
            return await _readerRepository.UpdateReaderPartialAsync(dto);
        }

        /**
         * 删锟斤拷一锟斤拷 Reader
         * @param readerID ReaderID
         * @return 锟斤拷影锟斤拷锟斤拷锟斤拷锟?
         */
        public async Task<int> DeleteReaderAsync(long readerID)
        {
            return await _readerRepository.DeleteReaderAsync(readerID);
        }

        public async Task<bool> RegisterReaderAsync(ReaderRegisterDto registerDto)
        {

            string userName = registerDto.UserName;
            string password = registerDto.Password;

            // string msg; // 未使用的变量，已注释

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("用户名和密码不能为空。");
            }
            else if (userName.Length < UserConstants.UsernameMinLength || userName.Length > UserConstants.UsernameMaxLength)
            {
                throw new ArgumentException($"用户名长度必须在{UserConstants.UsernameMinLength}到{UserConstants.UsernameMaxLength}之间。");
            }
            else if (password.Length < UserConstants.PasswordMinLength || password.Length > UserConstants.PasswordMaxLength)
            {
                throw new ArgumentException($"密码长度必须在{UserConstants.PasswordMinLength}到{UserConstants.PasswordMaxLength}之间。");
            }
            else if (IsUserNameExistsAsync(userName).Result)
            {
                throw new ArgumentException("用户名已存在，请选择其他用户名。");
            }

            Reader reader = new Reader
            {
                UserName = registerDto.UserName,
                Password = registerDto.Password
            };

            bool res = await _readerRepository.InsertReaderAsync(reader) > 0;
            if (!res)
            {
                throw new Exception("注册失败，请稍后再试。");
            }

            return res;
        }

        /**
         * 检查用户名是否已存在
         * @param userName 用户名
         * @return true 如果用户名已存在，否则 false
         */
        public async Task<bool> IsUserNameExistsAsync(string userName)
        {
            return await _readerRepository.IsUserNameExistsAsync(userName);
        }

        /**
         * 
         * 重置密码
         * 
         */
        public async Task<bool> ResetPasswordAsync(string userName, string newPassword)
        {
            return await _readerRepository.ResetPasswordAsync(userName, newPassword) > 0;
        }


        /**
         * 
         * 更新头像
         * 
         */
        public async Task<bool> UpdateAvatarAsync(long readerID, string avatarUrl)
        {
            return await _readerRepository.UpdateAvatarAsync(readerID, avatarUrl) > 0;
        }

        /**
         * 
         * 更新Reader的Profile字段
         * 
         */
        public async Task<bool> UpdateProfileAsync(Reader reader)
        {
            return await _readerRepository.UpdateProfileAsync(reader) > 0;
        }

    }
}