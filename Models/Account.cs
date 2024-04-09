using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using ThesisProject.Areas.Identity.Data;

namespace ThesisProject.Models
{
    public class Account
    {
        [PersonalData]
        public int? accountId { get; set; }
        [PersonalData]
        public string? accountName { get; set; }
        [PersonalData]
        [Required]
        public string? accountType { get; set; }
        [PersonalData]
        public long? accountNumber { get; set; }
        [PersonalData]
        public double? amount { get; set; }
        [PersonalData]
        public double?  accountLimit {  get; set; }
        [PersonalData]
        public double? accountInterest { get; set; }
        [PersonalData]
        [Required]
        public string? currencyType { get; set; }
        [DataType(DataType.Date)]
        [PersonalData]
        public DateTime? DateOfCreation { get; set; }
        [PersonalData]
        public int? cardId { get; set; }// foreign key
        [PersonalData]
        public string? userId { get; set; }// foreign key
        public ThesisProjectUser ThesisProjectUser { get; set; }// navigation property
        public Card Card { get; set; }// navigation property
        public Account() { }
    }
}
