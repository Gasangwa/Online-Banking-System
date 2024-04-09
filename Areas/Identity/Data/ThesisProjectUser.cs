using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using ThesisProject.Models;

namespace ThesisProject.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ThesisProjectUser class
public class ThesisProjectUser : IdentityUser
{
    
    [Required(ErrorMessage = "Name is required")]
    [StringLength(50, ErrorMessage = "Name must be less than 50 characters")]
    [PersonalData]
    public string? name { get; set; }

    [Required(ErrorMessage = "SurName is required")]
    [StringLength(50, ErrorMessage = "SurName must be less than 50 characters")]
    [PersonalData]
    public string? surName { get; set; }


    [Required(ErrorMessage = "Date of Birth is required")]
    [DataType(DataType.Date, ErrorMessage = "Invalid date format")]
    [PersonalData]
    public DateOnly? DOB { get; set; }
    [PersonalData]
    public string? street { get; set; }
    [PersonalData]
    public string? city { get; set; }
    [PersonalData]
    public string? state { get; set; }
    [PersonalData]
    public string? postalCode { get; set; }
    public string? accountStatus { get; set; }

    public ICollection<Account> Account { get; set; }// navigation property
    public ICollection<Card> Card { get; set; }// navigation property
    public ICollection<Transaction> Transaction { get; set; } // navigation property
    public ThesisProjectUser() {
        Account = new List<Account>();
        Card = new List<Card>();
        Transaction = new List<Transaction>();
    }
}

