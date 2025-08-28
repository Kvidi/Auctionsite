// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Auctionsite.Models.Database;

namespace Auctionsite.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public IndexModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Display(Name = "Användarnamn")]
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Telefonnummer")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Förnamn")]
            [Required(ErrorMessage = "Var god fyll i ditt förnamn.")]
            public string FirstName { get; set; }

            [Display(Name = "Efternamn")]
            [Required(ErrorMessage = "Var god fyll i ditt efternamn.")]
            public string LastName { get; set; }

            [Display(Name = "Organisationsnummer")]
            public string? OrgNr { get; set; }

            [Display(Name = "Gatuadress för leverans")]
            public string? DeliveryStreet { get; set; }

            [Display(Name = "Postkod för leverans")]
            public int? DeliveryZip { get; set; }

            [Display(Name = "Stad för leverans")]
            public string? DeliveryCity { get; set; }

            [Display(Name = "Gatuadress för betalning")]
            public string? BillingStreet { get; set; }

            [Display(Name = "Postkod för betalning")]
            public int? BillingZip { get; set; }

            [Display(Name = "Stad för betalning")]
            public string? BillingCity { get; set; }
        }

        private async Task LoadAsync(User user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                OrgNr = user.OrgNr,
                DeliveryStreet = user.DeliveryStreet,
                DeliveryZip = user.DeliveryZip,
                DeliveryCity = user.DeliveryCity,
                BillingStreet = user.BillingStreet,
                BillingZip = user.BillingZip,
                BillingCity = user.BillingCity
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (Input.PhoneNumber != user.PhoneNumber)
            {
                user.PhoneNumber = Helpers.PhoneNoHelper.FormatNr(Input.PhoneNumber);
            }
            if (Input.FirstName != user.FirstName)
            {
                user.FirstName = Input.FirstName;
            }
            if (Input.LastName != user.LastName)
            {
                user.LastName = Input.LastName;
            }
            if (Input.OrgNr != user.OrgNr)
            {
                user.OrgNr = Input.OrgNr;
            }
            if (Input.DeliveryStreet != user.DeliveryStreet)
            {
                user.DeliveryStreet = Input.DeliveryStreet;
            }
            if (Input.DeliveryZip != user.DeliveryZip)
            {
                user.DeliveryZip = Input.DeliveryZip;
            }
            if (Input.DeliveryCity != user.DeliveryCity)
            {
                user.DeliveryCity = Input.DeliveryCity;
            }
            if (Input.BillingStreet != user.BillingStreet)
            {
                user.BillingStreet = Input.BillingStreet;
            }
            if (Input.BillingZip != user.BillingZip)
            {
                user.BillingZip = Input.BillingZip;
            }
            if (Input.BillingCity != user.BillingCity)
            {
                user.BillingCity = Input.BillingCity;
            }
            

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Din profil har uppdaterats";
            if (string.IsNullOrEmpty(user.PhoneNumber) && !string.IsNullOrEmpty(Input.PhoneNumber))
            {
                StatusMessage += $", men {Input.PhoneNumber} är inte ett giltigt telefonnummer. Numret sparades inte.";
            }
            else
            {
                StatusMessage += $" med telefonnummer: {user.PhoneNumber}";
            }

            return RedirectToPage();
        }
    }
}
