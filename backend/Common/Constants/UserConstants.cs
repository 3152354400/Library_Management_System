namespace backend.Common.Constants
{
    public static class UserConstants
    {
        //用户名最短长度为2
        public const int UsernameMinLength = 2;

        //用户名最长长度为20
        public const int UsernameMaxLength = 20;

        //密码最短长度为5
        public const int PasswordMinLength = 5;

        //密码最长长度为20
        public const int PasswordMaxLength = 20;

        public const string AccuntStatusFrozen = "冻结"; //账户状态：冻结

        //用户类型读者
        public const string UserTypeReader = "reader";

        //用户类型管理员
        public const string UserTypeLibrarian = "librarian";

        //头像url为null
        public const string AvatarUrlNull = "null";

        //头像url为空
        public const string AvatarUrlEmpty = "";

        //头像url为默认头像
        public const string AvatarUrlDefault = "default";

        //系统头像0
        public const string SystemAvatar0 = "system_0.png";

        //系统头像1
        public const string SystemAvatar1 = "system_1.png";

        //系统头像2
        public const string SystemAvatar2 = "system_2.png";

        //系统头像3
        public const string SystemAvatar3 = "system_3.png";

        //系统头像4
        public const string SystemAvatar4 = "system_4.png";

        //系统头像5
        public const string SystemAvatar5 = "system_5.png";

        //系统头像6
        public const string SystemAvatar6 = "system_6.png";

        //系统头像7
        public const string SystemAvatar7 = "system_7.png";

        //系统头像8
        public const string SystemAvatar8 = "system_8.png";

        //系统头像9
        public const string SystemAvatar9 = "system_9.png";

        //系统头像10
        public const string SystemAvatar10 = "system_10.png";

        //文件大小1MB
        public const int FileSize1MB = 1 * 1024 * 1024; // 1MB

        //头像目录根路径
        public const string AvatarDirectoryRoot = "wwwroot/avatars";

        //密码通配符
        public const string PasswordAsterisk = "f8K3bPqX7tV2zR9nYwL1sHcD6mE4uJ0aQpG5oBvWkZxTjF3yN8rM\r\n";

        //默认密码
        public const string DefaultPassword = "1234567890";
    }
}
