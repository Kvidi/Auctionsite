// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Auctionsite.Models.Database;

namespace Auctionsite.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel(UserManager<User> userManager, IEmailSender emailSender) : PageModel
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly IEmailSender _emailSender = emailSender;

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

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
            [Required(ErrorMessage = "Var god fyll i din e-postadress.")]
            [EmailAddress(ErrorMessage = "Var god fyll i en giltig e-postadress.")]
            public string Email { get; set; } = string.Empty;
        }

        public void OnGet(string? email)
        {
            Input = new InputModel
            {
                Email = email ?? string.Empty
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Bekräftelsemejl skickat till din e-post. Vänligen kontrollera din inkorg.");
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);
            if (callbackUrl == null)
            {
                ModelState.AddModelError(string.Empty, "Ett fel uppstod när vi försökte generera bekräftelselänken. Försök igen senare.");
                return Page();
            }
            await _emailSender.SendEmailAsync(
                Input.Email,
                "Verifiera e-post",
                $"Vänligen verifiera din e-postadress genom att <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Klicka här</a>.");

            ModelState.AddModelError(string.Empty, "Bekräftelsemejl skickat till din e-post. Vänligen kontrollera din inkorg.");
            return Page();
        }
    }
}
