using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Admin
{
    public class HandleReportDto
    {
        [Required]
        [RegularExpression("^(approve|reject|处理完成|驳回)$")]
        public string Action { get; set; } = string.Empty;

        public bool BanUser { get; set; } = false;

        public string HandleResult { get; set; } = string.Empty;
    }
}
