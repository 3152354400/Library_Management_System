namespace backend.Models
{
    public class Librarian:User
    {
        public long LibrarianID { get; set; }
        public string StaffNo { get; set; }
        public string Password { get; set; }
        public string? Name { get; set; } = "";
        public string Permission { get; set; } = "普通";

        public string UserName
        {
            get { return StaffNo; }
            set { StaffNo = value; }
        }
    }
}
