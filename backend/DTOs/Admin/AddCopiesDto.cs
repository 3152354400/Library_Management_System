using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Admin
{
    public class AddCopiesDto
    {
        [Required]
        public string ISBN { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "入库数量必须在 1 到 100 之间")]
        public int NumberOfCopies { get; set; }

        [Required]
        public int ShelfID { get; set; }
    }
}