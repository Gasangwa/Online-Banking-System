using ThesisProject.Areas.Identity.Data;

namespace ThesisProject.Models
{
    public class Transaction
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public long accountNumber { get; set; }
        public double amount { get; set; }
        public string? currency {  get; set; }
        public string? Tway {  get; set; }
        public string userId {  get; set; }
        public DateTime dateOfTransaction {  get; set; } 
        public ThesisProjectUser ThesisProjectUser { get; set; }
        public Transaction() { }    
    }
}
