using System.Text.Json.Serialization;

namespace backend.DTOs.Reader
{
    public class ReaderDetailDto
    {
        [JsonPropertyName("userName")]
        public required string UserName { get; set; }

        [JsonPropertyName("fullName")]
        public string? FullName { get; set; }

        [JsonPropertyName("nickName")]
        public string? NickName { get; set; }

        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }

        [JsonPropertyName("creditScore")]
        public int CreditScore { get; set; }

        [JsonPropertyName("accountStatus")]
        public required string AccountStatus { get; set; }

        [JsonPropertyName("permission")]
        public required string Permission { get; set; }
    }
}
