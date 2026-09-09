using System;

namespace backend.DTOs.Admin
{
    public class PurchaseLogDto
    {
        public int LogID { get; set; }
        public string LogText { get; set; }
        public DateTime LogDate { get; set; }
    }
    
    public class CreatePurchaseLogDto
    {
        public string LogText { get; set; }
    }
}