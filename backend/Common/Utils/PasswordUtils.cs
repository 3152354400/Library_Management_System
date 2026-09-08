namespace backend.Common.Utils
{
    public static class PasswordUtils
    {
        /**
         * 匹配原始密码和加密密码
         * 
         */
        public static bool VerifyPassword(string rawPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(rawPassword, hashedPassword);
        }
        /**
         * 加密密码
         * 
         */
        public static string HashPassword(string rawPassword)
        {
            if (string.IsNullOrEmpty(rawPassword))
            {
                throw new ArgumentException("密码不能为空");
            }
            return BCrypt.Net.BCrypt.HashPassword(rawPassword);
        }
    }
}
