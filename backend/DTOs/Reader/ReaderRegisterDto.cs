using System.Text.Json.Serialization;

namespace backend.DTOs.ReaderDtos
{
    public class ReaderRegisterDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
      
        [JsonConstructor]
        public ReaderRegisterDto(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }
    }
}
