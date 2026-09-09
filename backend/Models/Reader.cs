
namespace backend.Models;

public class Reader: User
{
    public long ReaderID { get; set; }

    public required string UserName { get; set; }
    public required string Password { get; set; }

    public string? FullName { get; set; } = "";

    public string? NickName { get; set; } = "默认用户名";

    public string? Avatar { get; set; } = "";

    public int CreditScore { get; set; } = 100;

    public string AccountStatus { get; set; } = "正常";

    public string Permission { get; set; } = "普通";


}