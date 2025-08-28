using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Auctionsite.Models.Database;
using Auctionsite.Services.Interfaces;
using Auctionsite.Models.VM;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.Encodings.Web;

namespace Auctionsite.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IAdService _adService;
        private readonly IEmailSender _emailSender;
        private readonly IChatService _chatService;

        public AdminController(UserManager<User> userManager, IAdService adService, IEmailSender emailSender, IChatService chatService)
        {
            _userManager = userManager;
            _adService = adService;
            _emailSender = emailSender;
            _chatService = chatService;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Displays a list of all users in the system
        public async Task<IActionResult> AllUsers()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                if (!users.Any())
                {
                    throw new InvalidOperationException("No users found in the database.");
                }
                return View(users);
            }
            catch (Exception ex)
            {
                // Handle unexpected error
                return StatusCode(500, $"Error retrieving users: {ex.Message}");
            }
        }

        // Shows detailed information about a specific user
        public async Task<IActionResult> UserDetails(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("User ID is required.");
            }

            try
            { 
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound($"No user found with ID: {id}");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving user: {ex.Message}");
            }
        }

        // Shows all placed orders made by a specific user
        //public async Task<IActionResult> OrdersByUser(string id)
        //{
        //    if (string.IsNullOrWhiteSpace(id))
        //    {
        //        return BadRequest("User ID is required.");
        //    }

        //    try
        //    {
        //        var user = await _userManager.FindByIdAsync(id);
        //        if (user == null)
        //        {
        //            return NotFound("Användaren hittades inte.");
        //        }

        //        var orders = await _orderService.GetOrdersByUserIdAsync(id);

        //        ViewData["UserEmail"] = user.Email;

        //        return View(orders);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Error retrieving orders: {ex.Message}");
        //    }
        //}

        // Shows all placed orders across all users
        //public async Task<IActionResult> AllOrders()
        //{
        //    try
        //    {
        //        var orders = await _orderService.GetAllOrdersAsync();                
        //        return View(orders);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Error retrieving all orders: {ex.Message}");
        //    }
        //}


        // Shows all pending sales adds awaiting approval
        public async Task<IActionResult> ApprovePendingAds()
        {
            try
            {
                var pendingAds = await _adService.GetPendingAdvertisementsAsync();
                var vm = new ApprovePendingAdVM
                {
                    Ads = pendingAds.Select(ad => new PendingAdsListVM
                    {
                        Id = ad.Id,
                        Title = ad.Title,
                        UserName = ad.Advertiser.UserName,
                        AddedAt = ad.AddedAt,
                        IsSeenByAdmin = ad.IsSeenByAdmin
                    }).ToList()
                };
                return View(vm);
            }
            catch (Exception ex)
            {                
                return StatusCode(500, $"Error retrieving pending ads: {ex.Message}");
            }
        }

        // Approves a specific pending advertisement by ID
        [HttpPost]
        public async Task<IActionResult> ApproveAd(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid advertisement ID.");
            }
            try
            {
                var success = await _adService.ApproveAdAsync(id);
                if (!success)
                {
                    return NotFound($"Advertisement with ID {id} not found or already approved.");
                }
                TempData["Success"] = "Annonsen har godkänts.";
                return RedirectToAction("ApprovePendingAds");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error approving advertisement: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectAd(int id, string reason)
        {
            if (id <= 0)
                return BadRequest("Invalid advertisement ID.");

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "Ange alltid en orsak till avvisningen.";
                return RedirectToAction("ApprovePendingAds");
            }

            try
            {
                var ad = await _adService.GetAdByIdAsync(id);
                if (ad == null)
                    return NotFound($"Advertisment with ID {id} not found.");

                // Mark as rejected
                ad.IsRejected = true;
                ad.RejectionReason = reason;
                await _adService.UpdateAdAsync(ad);

                // System notification via chat
                var admin = await _userManager.GetUserAsync(User);
                bool isAdmin = await _userManager.IsInRoleAsync(admin, "Admin");
                var chat = await _chatService.GetOrCreateChatAsync(id, ad.Advertiser.Id);
                await _chatService.AddMessageAsync(
                    chat.Id,
                    admin.Id,
                    $"Din annons '<strong>{ad.Title}</strong>' har avvisats av en administratör.<br/>" +
                    $"<strong>Orsak:</strong> {reason}<br/>" +
                    $"Redigera gärna och skicka in den igen.<br/><br/>" +
                    $"<em>OBS! Detta är ett automatiserat meddelande. Du kan inte svara här.</em>",
                    allowHtml: isAdmin
                );

                // Email notification
                var editLink = Url.Action("EditAd", "Advertisement", new { id }, Request.Scheme);
                var emailContent = $@"
                Hej {ad.Advertiser.UserName},<br/><br/>
                Din annons '<strong>{ad.Title}</strong>' har avvisats av vår administratör.<br/>
                <strong>Orsak:</strong> {reason}<br/><br/>
                Du kan redigera och skicka in annonsen igen här:<br/>
                <a href='{HtmlEncoder.Default.Encode(editLink)}'>{editLink}</a><br/><br/>
                <em>OBS! Detta är ett automatiserat mejl. Du kan inte svara på detta meddelande.</em><br/><br/>
                Med vänlig hälsning,<br/>Auctionsite-teamet";

                await _emailSender.SendEmailAsync(
                    ad.Advertiser.Email,
                    "Din annons har tyvärr avvisats",
                    emailContent);

                TempData["Success"] = "Annonsen har avvisats och användaren har informeras via mail och meddelande.";
                return RedirectToAction("ApprovePendingAds");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error rejecting advertisement: {ex.Message}");
            }
        }

        // Get statistics for admin dashboard 
        public async Task<IActionResult> AdminStatistics()
        {
            try
            {                
                return View();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error loading statistics view: {ex.Message}");
            }
        }

        // VAT configuration page
        public async Task<IActionResult> VatSettings()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error loading VAT settings: {ex.Message}");
            }
        }
    }
}
