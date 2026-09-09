using backend.Models;
using System.Text.Json.Serialization;

namespace backend.DTOs.Web
{
    public class LoginUser
    {
        public User User { get; set; }

        public string? UserType { get; set; } 

        public string UserName
        {
            get { return User.UserName; }
            set { User.UserName = value; }
        }

        public string Password
        {
            get { return User.Password; }
            set { User.Password = value; }
        }




        //唯一标识
        public string? Token { get; set; }

        //登陆时间
        public long LoginTime { get; set; }

        //过期时间
        public long ExpireTime { get; set; }

        [JsonConstructor]
        public LoginUser(User user)
        {
            this.User= user;
        }

        public LoginUser(User user, string userType)
        {
            this.User = user;
            this.UserType = userType;
        }

    }
}
