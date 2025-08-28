// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Claims;
using Auctionsite.Helpers;
using Auctionsite.Models.Database;


namespace Auctionsite.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IUserStore<User> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
        }

        //[BindProperty]
        //public string PhoneNumber { get; set; }
        //[BindProperty]
        //public string Address { get; set; }
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
        public string ProviderDisplayName { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

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
            [Required(ErrorMessage = "Var god fyll i ditt förnamn.")]
            [Display(Name = "Förnamn")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Var god fyll i ditt efternamn.")]
            [Display(Name = "Efternamn")]
            public string LastName { get; set; }
            [Required(ErrorMessage = "E-postadress saknas.")]
            [EmailAddress(ErrorMessage = "Ej en giltig e-postadress.")]
            [Display(Name = "E-post")]
            public string Email { get; set; }
            [Display(Name = "Telefonnummer")]
            public string PhoneNumber { get; set; }
            [Display(Name = "Bostadsadress")]
            public string Address { get; set; }
            [Required(ErrorMessage = "Vänligen ange födelsedatum.")]
            [Display(Name = "Födelsedatum (sparas ej)")]
            [DataType(DataType.Date)]
            public DateTime Dob { get; set; }

        }

        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                var token = info.AuthenticationTokens.FirstOrDefault().Value;
                Console.WriteLine("Access token in OnGetCallbackAsync(): " + token);
                if (token != null)
                {
                    var externalInfo = await FetchGoogleUserDetails(token);
                    if (externalInfo != null)
                    {
                        ErrorMessage = $"Registreringen med {ProviderDisplayName} misslyckades.";
                        return RedirectToPage("./Login", new { ReturnUrl = returnUrl});
                    }
                }
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    var userEmail = info.Principal.FindFirstValue(ClaimTypes.Email);
                    var userExist = await _userManager.FindByEmailAsync(userEmail);
                    if (userExist != null)
                    {
                        ErrorMessage =
                            $"e-postadressen {userEmail} finns redan registrerad, men inte kopplad till ditt {ProviderDisplayName}-konto." +
                            $" För att koppla ihop ditt konto med {ProviderDisplayName}, vänligen logga in och ändra inställningarna under:" +
                            $"\" Mina Sidor ► Inställningar ► Externa inloggningar\"";

                        return RedirectToPage("./Login", new { ReturnUrl = returnUrl});
                    }
                    Input = new InputModel
                    {
                        Email = userEmail,
                        FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName),
                        LastName = info.Principal.FindFirstValue(ClaimTypes.Surname),
                        Address = HttpContext.Session.GetString("addr") ?? "",
                        PhoneNumber = HttpContext.Session.GetString("phoneNo") ?? "",
                        Dob = DateTime.ParseExact(HttpContext.Session.GetString("dob"), "yyyy-MM-dd", CultureInfo.InvariantCulture)
                    };
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            if (ModelState.IsValid)
            {
                if (!AgeVerificationHelper.VerifyAge(Input.Dob))
                {
                    ErrorMessage = "Du måste vara minst 16 år för att skapa ett konto.";
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }

                string formatedPhoneNo = PhoneNoHelper.FormatNr(Input.PhoneNumber);
                if (string.IsNullOrEmpty(formatedPhoneNo) && !string.IsNullOrEmpty(Input.PhoneNumber))
                {
                    TempData["TelephoneError"] = "\"Telefonnumret angavs felaktigt. För att lägga till det, gå till: \"Mina Sidor ► Inställningar ► Profil.\"";
                    _logger.LogInformation("Failed to format phone number. Number not saved in database");
                }

                var user = CreateUser();
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.PhoneNumber = formatedPhoneNo;
                Input.Email = info.Principal.FindFirstValue(ClaimTypes.Email);
                user.DeliveryStreet = Input.Address;
                user.BillingStreet = Input.Address;
                user.EmailConfirmed = true;
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        /*
                         * Section bellow removed since access to he email is already confirmed by current login provider (Google) by issuing a valid token. 
                         */

                        //var userId = await _userManager.GetUserIdAsync(user);
                        //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        //var callbackUrl = Url.Page(
                        //    "/Account/ConfirmEmail",
                        //    pageHandler: null,
                        //    values: new { area = "Identity", userId = userId, code = code },
                        //    protocol: Request.Scheme);

                        //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        //// If account confirmation is required, we need to show the link if we don't have a real email sender
                        //if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        //{
                        //    return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        //}

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        private User CreateUser()
        {
            try
            {
                return Activator.CreateInstance<User>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(User)}'. " +
                    $"Ensure that '{nameof(User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore;
        }
#nullable enable
        private async Task<IActionResult?> FetchGoogleUserDetails(string accessToken)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.GetAsync(
                    "https://people.googleapis.com/v1/people/me?personFields=addresses,phoneNumbers,birthdays");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var person = JObject.Parse(content);

                string address = person["addresses"]?
                    .FirstOrDefault()?["formattedValue"]?
                    .ToString()?
                    .Trim() ?? string.Empty;

                string phoneNumber = person["phoneNumbers"]?
                    .FirstOrDefault()?["value"]?
                    .ToString()?
                    .Trim() ?? string.Empty;

                DateTime? birthDate = null;
                var birthday = person["birthdays"]?.FirstOrDefault()?["date"];
                if (birthday != null)
                {
                    try
                    {
                        var birthYear = (int?)birthday["year"] ?? 0;
                        var birthMonth = (int?)birthday["month"] ?? 1;
                        var birthDay = (int?)birthday["day"] ?? 1;

                        if (birthYear > 1900 && birthYear <= DateTime.Today.Year)
                        {
                            birthDate = new DateTime(birthYear, birthMonth, birthDay);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse birthdate from Google response");
                    }
                }

                HttpContext.Session.SetString("addr", address);
                HttpContext.Session.SetString("phoneNo", phoneNumber);
                if (birthDate.HasValue)
                {
                    HttpContext.Session.SetString("dob", birthDate.Value.ToString("yyyy-MM-dd"));
                }
                return null;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Failed to fetch user details from Google API");
                return RedirectToAction("OAuthError", new { message = "Failed to communicate with Google" });
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Failed to parse Google API response");
                return RedirectToAction("OAuthError", new { message = "Invalid data received from Google" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching Google user details");
                return RedirectToAction("Error");
            }
        }
    }
}
