using System.Collections.Generic;

namespace Backend.DTOs.Book
{
    public class GetBooklistDetailsResponse
    {
        public BooklistInfoDto? BooklistInfo { get; set; }
        public List<BookItemDto> Books { get; set; } = new();
    }
}
