using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ThesisProject.Areas.Identity.Data;

namespace ThesisProject.Areas.Identity.Pages.Account.Manage
{
    public class AddressModel : PageModel
    {
        private readonly UserManager<ThesisProjectUser> _userManager;
        public AddressModel(UserManager<ThesisProjectUser> userManager)
        {
            _userManager = userManager;
        }

        public string? street { get; set; }
        public string? city { get; set; }
        public string? state { get; set; }
        public string? zipCode { get; set; }
        [TempData]
        public string StatusMessage { get; set; }
        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Street")]
            public string? street { get; set; }
            [Display(Name = "City")]
            [Required]
            public string? city { get; set; }
            [Required]
            [Display(Name = "State")]
            public string? state { get; set; }
            [Required]
            [Display(Name = "Zipcode")]
            [RegularExpression(@"^\d{2}(-\d{3})?$", ErrorMessage = "Invalid Zip Code")]

            public string? zipCode { get; set; }
        }
        private void LoadAsync(ThesisProjectUser user)
        {
            if (user != null)
            {
                street = user.street;
                city = user.city;
                state = user.state;
                zipCode = user.postalCode;
            }
            Input = new InputModel
            {
                street = street,
                city = city,
                state = state,
                zipCode = zipCode
            };

        }
        public IActionResult OnGetAddressAsync()
        {
            var user = _userManager.Users.FirstOrDefault(u => u.Id == _userManager.GetUserId(User));
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            LoadAsync(user);
            return Page();
        }
        public async Task<IActionResult> OnPostAddressAsync()
        {
            var user = _userManager.Users.FirstOrDefault(u => u.Id == _userManager.GetUserId(User));
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            if (!ModelState.IsValid)
            {
                LoadAsync(user);
                return Page();
            }
            if (user.state != Input.state || user.city != Input.city || user.street != Input.street || user.postalCode != Input.zipCode)
            {
                user.state = Input.state;
                user.street = Input.street;
                user.city = Input.city;
                user.postalCode = Input.zipCode;
                await _userManager.UpdateAsync(user);
                StatusMessage = "Your new address have been added";
                return RedirectToAction("Settings", "Accounts", new {id=_userManager.GetUserId(User)});
            }
            StatusMessage = "No change made on your address";
            return RedirectToPage();
        }
    }
}
