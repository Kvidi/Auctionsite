using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NuGet.Packaging.Signing;
using System.Diagnostics.Tracing;
using System.Security.Claims;
using Auctionsite.Data;
using Auctionsite.Helpers;
using Auctionsite.Hubs;
using Auctionsite.Models;
using Auctionsite.Models.Bids;
using Auctionsite.Models.Database;
using Auctionsite.Models.Requests;
using Auctionsite.Models.VM;
using Auctionsite.Services;
using Auctionsite.Services.Interfaces;

namespace Auctionsite.Controllers
{
    public class AdvertisementController : Controller
    {
        private readonly IAdService _adService;
        private readonly ICategoryService _categoryService;
        private readonly IChatService _chatService;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AdvertisementController> _logger;
        private readonly IHubContext<AdvertisementHub> _adHubContext;
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        private readonly INotificationService _notificationService;
        private readonly IMemoryCache _cache;
        private readonly IWebHostEnvironment _env;

        public AdvertisementController(IAdService adService, ICategoryService categoryService, IChatService chatService, INotificationService notificationService, 
            UserManager<User> userManager, ApplicationDbContext db, ILogger<AdvertisementController> logger, IHubContext<AdvertisementHub> adHubContext, IHubContext<NotificationHub> notificationHubContext, IMemoryCache cache, IWebHostEnvironment env)
        {
            _adService = adService;
            _categoryService = categoryService;
            _chatService = chatService;
            _notificationService = notificationService;
            _userManager = userManager;
            _db = db;
            _logger = logger;
            _adHubContext = adHubContext;
            _notificationHubContext = notificationHubContext;
            _cache = cache;
            _env = env;
        }

        // Index page for ads
        [HttpGet]
        public async Task<IActionResult> Index(int? categoryId, int page = 1)
        {
            const int pageSize = 15;

            // Create an empty filter for the initial page load
            var filter = new AdSearchFilterViewModel
            {
                CategoryId = categoryId
            };

            var pagedAd = await _adService.GetAdsByFilterAsync(filter, page, pageSize);

            // Pass the empty filter to the view
            ViewBag.SearchFilter = filter;
            ViewBag.CategoryId = categoryId;

            // Load filter options
            ViewBag.ConditionOptions = SelectListHelper.GetConditionOptions();
            ViewBag.AdTypeOptions = SelectListHelper.GetAdTypeOptions();
            ViewBag.GroupedCategories = await _categoryService.GetGroupedSubCategoriesAsync();

            // Handle breadcrumbs if category is specified
            if (categoryId.HasValue)
            {
                ViewBag.CategoryBreadcrumbs = await _categoryService.GetCategoryBreadcrumbsAsync(categoryId.Value);
            }

            // Get user favourites (same as in Search method)
            User? user = null;
            if (User.Identity.IsAuthenticated)
            {
                user = await _userManager.GetUserAsync(User);
            }

            var favIds = new HashSet<int>();
            if (user != null)
            {
                favIds = await _db.Advertisements
                    .Where(a => a.UsersWhoFavourited.Any(u => u.Id == user.Id))
                    .Select(a => a.Id)
                    .ToHashSetAsync();
            }

            // Map to view models
            var vmItems = pagedAd.Items.Select(ad => new AdCardVM
            {
                Id = ad.Id,
                Title = ad.Title,
                HeadImageUrl = ad.Images.Where(img => img.IsMain).Select(img => img.Url).FirstOrDefault() ?? string.Empty,
                AuctionEndDate = ad.AuctionEndDate,
                AdType = ad.AdType,
                StartingPrice = ad.StartingPrice,
                BuyNowPrice = ad.BuyNowPrice,   
                CurrentHighestBid = ad.CurrentHighestBid,
                BidCount = ad.Bids?.Count ?? 0,
                IsFavourite = favIds.Contains(ad.Id),
                ApprovedAt = ad.ApprovedAt
            }).ToList();

            var vmPaged = new PagedResult<AdCardVM>
            {
                Items = vmItems,
                PageNumber = pagedAd.PageNumber,
                PageSize = pagedAd.PageSize,
                TotalItems = pagedAd.TotalItems
            };

            return View(vmPaged);
        }


        // Create ad
        [HttpGet("skapa-annons")]
        public async Task<IActionResult> CreateAd(int? categoryId = null)
        {
            var vm = new AdFormVM();
            if (categoryId.HasValue)
                vm.CategoryId = categoryId.Value;

            // Fetch the main categories for the dropdown (ParentCategoryId == null)
            var mainCategories = await _categoryService.GetSubCategoriesAsync(null);
            ViewBag.MainCategories = mainCategories;

            await RefillDropdownsAsync(vm);
            return View(vm);
        }

        // Create ad - POST
        [HttpPost("skapa-annons")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAd(AdFormVM vm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (!ModelState.IsValid)
            {
                // Fetch the main categories and refill the dropdowns if validation fails
                var mainCategories = await _categoryService.GetSubCategoriesAsync(null);
                ViewBag.MainCategories = mainCategories;
                await RefillDropdownsAsync(vm);
                return View(vm);               
            }

            var urls = vm.ImageUrls.Take(10).ToList();
                        
            // Create image entities
            var images = urls
                .Select((u, idx) => new AdvertisementImage
                {
                    Url = u,
                    Order = idx,
                    IsMain = (idx == 0)
                })
                .ToList();

            // Mapping VM → entity
            var ad = new Advertisement
            {
                Advertiser = user,
                CategoryId = vm.CategoryId,
                Title = vm.Title,
                Description = vm.Description,
                Condition = vm.Condition.GetValueOrDefault(),
                AdType = vm.AdType.GetValueOrDefault(),
                StartingPrice = vm.StartingPrice,
                BuyNowPrice = vm.BuyNowPrice,
                MinimumEndPrice = vm.MinimumEndPrice,
                Images = images,
                VideoURL = vm.VideoURL,
                AvailableForPickup = vm.AvailableForPickup,
                PickupLocation = vm.PickupLocation,
                AddedAt = DateTime.Now,                
                //ShippingMethod = vm.ShippingMethod,
                //ShippingCost = vm.ShippingCost,
                //PaymentMethods = vm.PaymentMethods,
                AuctionEndDate = vm.AuctionEndDate,
                IsCompanySeller = vm.IsCompanySeller                
            };

            try
            {
                var result = await _adService.CreateAdAsync(ad);
                if (result == null)
                {
                    TempData["Error"] = "Ett oväntat fel inträffade när annonsen skulle sparas.";
                    return RedirectToAction(nameof(CreateAd));
                }
                                
                TempData["Success"] = $"Annonsen \"{result.Title}\" är skapad!";
                return RedirectToAction(nameof(CreateAd));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ett fel inträffade vid sparande: " + ex.Message;
                return RedirectToAction(nameof(CreateAd));
            }
        }

        // Details page for an ad
        [Route("annonser/{id:int}")]
        public async Task<IActionResult> Details(int id, bool isFavourite)
        {
            var ad = await _adService.GetAdByIdAsync(id);
            if (ad == null)
                return NotFound("Annonsen hittades inte.");

            var user = await _userManager.GetUserAsync(User);
            bool isFav = false;
            if (user != null)
            {                
                isFav = ad.UsersWhoFavourited.Any(u => u.Id == user.Id);                
            }

            bool wasBought = ad.PurchasedAt.HasValue;

            var leadingBidderId = await _adService.GetLeadingBidderIdAsync(id);
            
            var vm = new AdDetailsVM
            {
                Id = ad.Id,
                Title = ad.Title,
                Description = ad.Description,

                StartingPrice = ad.StartingPrice,
                MinimumEndPrice = ad.MinimumEndPrice,
                BuyNowPrice = ad.BuyNowPrice,

                CurrentHighestBid = ad.CurrentHighestBid,
                LeadingMaxbid = ad.MaxBids?.FirstOrDefault(mb => mb.UserId == leadingBidderId)?.Amount ?? 0, 
                MinimumBid = await _adService.GetMinimumBidAmountAsync(ad.Id),
                BidCount = ad.Bids?.Count ?? 0,
                IsLeadingBidder = user != null && leadingBidderId == user.Id,
                IsOutbid = user != null && await _adService.IsOutBid(ad.Id, user.Id),
                LeadingBidderUserName = leadingBidderId != null ? (await _userManager.FindByIdAsync(leadingBidderId))?.UserName : null,

                HeadImageURL = ad.Images
                    .Where(img => img.IsMain)
                    .Select(img => img.Url)
                    .FirstOrDefault()
                    ?? string.Empty,
                VideoURL = ad.VideoURL,
                Images = ad.Images.ToList(),

                AddedAt = ad.AddedAt,
                AuctionEndDate = ad.AuctionEndDate,

                IsCompanySeller = ad.IsCompanySeller,
                AvailableForPickup = ad.AvailableForPickup,
                PickupLocation = ad.PickupLocation,

                Condition = ad.Condition,
                AdType = ad.AdType,

                WasBought = wasBought,
                PurchasedAt = ad.PurchasedAt,
                PurchasedByUserName = ad.PurchasedBy?.UserName,

                UserName = ad.Advertiser.UserName,
                IsFavourite = isFav,
                ApprovedAt = ad.ApprovedAt,
                IsRejected = ad.IsRejected,

                //ShippingMethod = ad.ShippingMethod,
                //ShippingCost = ad.ShippingCost,                
                //PaymentMethods = ad.PaymentMethods.Select(pm => pm.Name).ToList(),

                Breadcrumbs = await _categoryService.GetCategoryBreadcrumbsAsync(ad.CategoryId),
                SubCategories = await _categoryService.GetSubCategoriesAsync(ad.CategoryId)               
            };
            // Increment view count
            ad.ViewCount++;
            _db.Advertisements.Update(ad);
            await _db.SaveChangesAsync();

            return View(vm);
        }

        // Edit ad
        [Route("redigera-annons/{id:int}")]
        public async Task<IActionResult> EditAd(int id)
        {
            var ad = await _adService.GetAdByIdAsync(id);
            if (ad == null)
            {
                return NotFound("Annonsen hittades inte.");
            }

            // Mapping entity → VM
            var vm = new AdFormVM
            {
                Id = ad.Id,
                Title = ad.Title,
                Description = ad.Description,
                CategoryId = ad.CategoryId,
                CategoryName = ad.Category.Name,
                Breadcrumbs = await _categoryService.GetCategoryBreadcrumbsAsync(ad.CategoryId),
                Condition = ad.Condition,
                AdType = ad.AdType,
                StartingPrice = ad.StartingPrice,
                MinimumEndPrice = ad.MinimumEndPrice,
                BuyNowPrice = ad.BuyNowPrice,
                Images = ad.Images
                    .OrderBy(i => i.Order)
                    .Select(i => new ImageDto
                    {
                        Id = i.Id,
                        Url = i.Url
                    })
                    .ToList(),
                ImageUrls = ad.Images.OrderBy(i => i.Order).Select(i => i.Url).ToList(),
                ImageOrder = ad.Images.OrderBy(i => i.Order)
                        .Select(i => $"{i.Url}:{i.Order}")
                        .ToList(),
                VideoURL = ad.VideoURL,
                AvailableForPickup = ad.AvailableForPickup,
                PickupLocation = ad.PickupLocation,
                AuctionEndDate = ad.AuctionEndDate,
                IsCompanySeller = ad.IsCompanySeller,
                IsRejected = ad.IsRejected,
                RejectionReason = ad.RejectionReason,
                AddedAt = ad.AddedAt,
                IsSeenByAdmin = ad.IsSeenByAdmin
            };

            // Fill dropdownlists
            var mainCats = await _categoryService.GetSubCategoriesAsync(null);
            ViewBag.MainCategories = mainCats;
            ViewBag.Categories = new SelectList(mainCats, "Id", "Name");
            await RefillDropdownsAsync(vm);

            return View(vm);
        }

        // Edit ad - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("redigera-annons/{id:int}")]
        public async Task<IActionResult> EditAd(AdFormVM vm)
        {            
            if (!ModelState.IsValid)
            {                                
                // Fill dropdownlists
                var mainCats = await _categoryService.GetSubCategoriesAsync(null);
                ViewBag.MainCategories = mainCats;
                ViewBag.Categories = new SelectList(mainCats, "Id", "Name");
                await RefillDropdownsAsync(vm);
                return View(vm);
            }

            // Get the ad from the database
            var existing = await _adService.GetAdByIdAsync(vm.Id);
            if (existing == null)
                return NotFound("Annonsen hittades inte.");

            // Delayed removal of images
            if (vm.DeletedImageUrls?.Any() == true)
            {
                foreach (var url in vm.DeletedImageUrls)
                {
                    // Take away the entity‐post if it exists
                    var imgEntity = existing.Images.FirstOrDefault(i => i.Url == url);
                    if (imgEntity != null)
                    {
                        existing.Images.Remove(imgEntity);
                    }
                    // Delete the image from disk
                    await _adService.DeleteImageAsync(url);
                }
            }

            // Keep track of the URLs that should be kept
            var keptUrls = vm.ImageUrls ?? new List<string>();

            // Remove images that are not in the kept URLs
            var remove = existing.Images.Where(i => !keptUrls.Contains(i.Url)).ToList();
            foreach (var img in remove)
                existing.Images.Remove(img);

            // Update order, IsMain and add new images
            for (int idx = 0; idx < keptUrls.Count; idx++)
            {
                var url = keptUrls[idx];
                var imgEntity = existing.Images.FirstOrDefault(i => i.Url == url);
                if (imgEntity != null)
                {
                    imgEntity.Order = idx;
                    imgEntity.IsMain = (idx == 0);
                }
                else
                {
                    // New image - create entity
                    existing.Images.Add(new AdvertisementImage
                    {
                        Url = url,
                        Order = idx,
                        IsMain = (idx == 0)
                    });
                }
            }

            // Mapp VM → entity
            existing.Title = vm.Title;
            existing.Description = vm.Description;
            existing.CategoryId = vm.CategoryId;
            existing.Condition = vm.Condition.GetValueOrDefault();
            existing.AdType = vm.AdType.GetValueOrDefault();
            existing.StartingPrice = vm.StartingPrice;
            existing.MinimumEndPrice = vm.MinimumEndPrice;
            existing.BuyNowPrice = vm.BuyNowPrice;            
            existing.VideoURL = vm.VideoURL;            
            existing.AvailableForPickup = vm.AvailableForPickup;
            existing.PickupLocation = vm.PickupLocation;
            existing.AuctionEndDate = vm.AuctionEndDate;
            existing.IsCompanySeller = vm.IsCompanySeller;
            existing.IsRejected = false;
            existing.RejectionReason = null;
            existing.AddedAt = DateTime.Now;
            existing.IsSeenByAdmin = false;

            try
            {
                var updated = await _adService.UpdateAdAsync(existing);
                
                if (updated)
                {
                    TempData["Success"] = "Annonsen uppdaterades!";
                    return RedirectToPage("/Account/Manage/MinaAnnonser", new { area = "Identity" });
                }
                TempData["Error"] = "Kunde inte uppdatera annonsen.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ett fel inträffade när annonsen skulle uppdateras." + ex.Message;
            }
            // Refill dropdownlists
            var mainCatsReload = await _categoryService.GetSubCategoriesAsync(null);
            ViewBag.MainCategories = mainCatsReload;
            ViewBag.Categories = new SelectList(mainCatsReload, "Id", "Name");
            await RefillDropdownsAsync(vm);
            return View(vm);
        }

        // Delete ad
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAd(int id)
        {
            var success = await _adService.DeleteAdAsync(id);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") // Check if the request is an AJAX call
            {
                // If the request is an AJAX call, return JSON
                // For use when the user clicks the delete button from a list of ads
                return Json(new
                {
                    success,
                    message = success ? "Annonsen har tagits bort." : "Misslyckades med att ta bort annonsen."
                });
            }

            // Regular form submission
            // When the user clicks the delete button from the ad-details page
            if (success)
            {
                TempData["Success"] = "Annonsen har tagits bort.";
            }
            else
            {
                TempData["Error"] = "Misslyckades med att ta bort annonsen.";
            }

            return RedirectToPage("/Account/Manage/MinaAnnonser", new { area = "Identity" });
        }

        // Toggle ad between favourited and unfavourited
        // This is used when the user clicks the heart icon beside an ad        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ToggleFavourite([FromBody] ToggleFavouriteRequest req)
        {
            // Get the advertisement ID from the request
            var adId = req.AdvertisementId;
            // Get the logged in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // Get the advertisement
            var advertisement = await _adService.GetAdByIdAsync(adId);
            if (advertisement == null)
            {
                return NotFound();
            }

            // Check if the user already favourited this ad
            var alreadyFavourited = user.FavouriteAds.Any(a => a.Id == adId);

            // If the user already favourited the ad, remove it from the user's favourites
            if (alreadyFavourited)
            {
                user.FavouriteAds.Remove(advertisement);
                await _db.SaveChangesAsync();
                return Ok(new { success = true, favourited = false });
            }
            else
            {
                user.FavouriteAds.Add(advertisement);
                await _db.SaveChangesAsync();
                return Ok(new { success = true, favourited = true });
            }
        }

        // My ads page
        [Route("mina-annonser")]
        [Authorize]
        public async Task<IActionResult> MyAds()
        {
            var user = await _userManager.GetUserAsync(User);
            var ads = await _adService.GetAdsByUserIdAsync(user.Id, 1, 10);

            return View(ads);
        }

        // My favourite ads page
        [Authorize]
        public async Task<IActionResult> Favourites()
        {
            var user = await _userManager.GetUserAsync(User);
            var favourites = await _adService.GetFavourites(user.Id, 1, 10);

            return View(favourites);
        }



        [HttpGet]
        [Route("annonser/sok")]
        public async Task<IActionResult> Search([FromQuery] AdSearchFilterViewModel filter, int page = 1)
        {
            const int pageSize = 15;
            var pagedAd = await _adService.GetAdsByFilterAsync(filter, page, pageSize);

            // IMPORTANT: Pass the filter back to the view so the view can access filter values
            // This eliminates the need to use Request.Query in the view
            ViewBag.SearchFilter = filter;

            // Load filter options needed by the Index view
            ViewBag.ConditionOptions = SelectListHelper.GetConditionOptions();
            ViewBag.AdTypeOptions = SelectListHelper.GetAdTypeOptions();

            // When searching, we can provide all parent categories as a starting point for the filter
            ViewBag.GroupedCategories = await _categoryService.GetGroupedSubCategoriesAsync();

            // Get the currently logged in user to check for favourites
            User? user = null;
            if (User.Identity.IsAuthenticated)
            {
                user = await _userManager.GetUserAsync(User);
            }

            // Retrieve the IDs of advertisements that the user has favourited
            var favIds = new HashSet<int>();
            if (user != null)
            {
                favIds = await _db.Advertisements
                    .Where(a => a.UsersWhoFavourited.Any(u => u.Id == user.Id))
                    .Select(a => a.Id)
                    .ToHashSetAsync();
            }

            // Map Advertisement → AdCardVM
            var vmItems = pagedAd.Items.Select(ad => new AdCardVM
            {
                Id = ad.Id,
                Title = ad.Title,
                HeadImageUrl = ad.Images.Where(img => img.IsMain).Select(img => img.Url).FirstOrDefault() ?? string.Empty,
                AuctionEndDate = ad.AuctionEndDate,
                AdType = ad.AdType,
                StartingPrice = ad.StartingPrice,
                BuyNowPrice = ad.BuyNowPrice,
                CurrentHighestBid = ad.CurrentHighestBid,
                BidCount = ad.Bids?.Count ?? 0,
                IsFavourite = favIds.Contains(ad.Id),
                ApprovedAt = ad.ApprovedAt
            }).ToList();

            // Build the paged result for the view model
            var vmPaged = new PagedResult<AdCardVM>
            {
                Items = vmItems,
                PageNumber = pagedAd.PageNumber,
                PageSize = pagedAd.PageSize,
                TotalItems = pagedAd.TotalItems
            };

            return View("Index", vmPaged);
        }


        // Mapping method to convert Advertisement to AdCardVM
        private AdCardVM MapToAdCardVM(Advertisement ad, bool isFavourite)
        {
            return new AdCardVM
            {
                Id = ad.Id,
                Title = ad.Title,
                HeadImageUrl = ad.Images
                    .Where(img => img.IsMain)
                    .Select(img => img.Url)
                    .FirstOrDefault()
                    ?? string.Empty,
                AuctionEndDate = ad.AuctionEndDate,
                AdType = ad.AdType,
                StartingPrice = ad.StartingPrice,
                BuyNowPrice = ad.BuyNowPrice,
                CurrentHighestBid = ad.CurrentHighestBid,
                BidCount = ad.Bids?.Count ?? 0,
                IsFavourite = isFavourite,
                ApprovedAt = ad.ApprovedAt,
                IsRejected = ad.IsRejected
            };
        }


        // Returns a partial view (modal) with the bid history for an ad
        public async Task<IActionResult> BidHistoryPartial (int adId)
        {
            var bidHistory = await _adService.GetBidHistoryAsync(adId);
            return PartialView("_BidHistoryPartial", bidHistory);
        }

        // Place a bid on an ad
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceBid(int adId, decimal maxBidAmount, decimal minimumBid)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized(new { success = false, message = "Du måste vara inloggad för att lägga ett bud." });

            var previousLeadingBidderId = await _adService.GetLeadingBidderIdAsync(adId);

            var result = await _adService.PlaceMaxBidAsync(adId, userId, maxBidAmount);

            var newLeadingBidderId = await _adService.GetLeadingBidderIdAsync(adId);
            var newLeadingBidder = newLeadingBidderId != null ? await _userManager.FindByIdAsync(newLeadingBidderId) : null;
            var newLeadingBidderUserName = newLeadingBidder?.UserName;
            bool isNewLeader = previousLeadingBidderId != newLeadingBidderId; // Check if the leading bidder has changed

            var isLeading = newLeadingBidderId == userId;
            var ad = await _adService.GetAdByIdAsync(adId);
            var adTitle = ad?.Title ?? "Annonsen har tagits bort";
            var newLeadingVisibleBid = await _adService.GetLeadingVisibleBidAmountAsync(adId);
            var minimumBidAmount = await _adService.GetMinimumBidAmountAsync(adId);
            var bidCount = await _adService.GetBidCountAsync(adId);

            var outBidUserIds = await _adService.GetOutbidUserIdsAsync(adId);

            // If there is a new leader
            if (isNewLeader && newLeadingBidderId != null)
            {
                // If there are users who have been outbid, we need to notify them
                if (outBidUserIds.Count > 0)
                {
                    // Create notifications for all users who have been outbid
                    await _notificationService.CreateOutbidNotificationsAsync(outBidUserIds, adId, ad.Title, newLeadingBidderUserName, (decimal)newLeadingVisibleBid, userId);

                    // Send realtime notifications to outbid users (for the navbar)
                    foreach (var outbidUserId in outBidUserIds)
                    {
                        await _notificationHubContext.Clients
                            .User(outbidUserId)
                            .SendAsync("ReceiveNotification");
                    }
                }

                // Notify the advertiser that a new leading bid has been placed
                if (ad?.Advertiser?.Id != null && ad.Advertiser.Id != userId)
                {
                    await _notificationService.CreateNewLeadingBidNotificationAsync(ad.Advertiser.Id, adId, adTitle, (decimal)newLeadingVisibleBid);

                    await _notificationHubContext.Clients
                        .User(ad.Advertiser.Id)
                        .SendAsync("ReceiveNotification");
                }
            }

            // Send a real-time update to users who are viewing the ad. Is processed in javascript.
            if (result.Success)
            {
                await _adHubContext.Clients.Group($"ad-{adId}").SendAsync("ReceiveBidUpdate", new
                {
                    previousLeadingBidderId,
                    maxBidAmount,
                    currentHighestBid = newLeadingVisibleBid,
                    bidCount,
                    minimumBidAmount,
                    leadingBidderId = newLeadingBidderId,    
                    leadingBidderUserName = newLeadingBidderUserName,
                    outBidUserIds
                });
            }

            // Return JSON response for AJAX requests. For the user who placed the bid
            if (result.Success && (result.Error == PlaceBidError.None || result.Error == PlaceBidError.AlreadyLeading))
            {
                // If the bid was successful, we return a success message and the updated bid information
                // Head and sub messages will be displayed in the UI as a toast or alert
                var headMessage = result.Error switch
                {
                    PlaceBidError.None => $"Du leder med {newLeadingVisibleBid:N0} kr",
                    PlaceBidError.AlreadyLeading => $"Du leder med {newLeadingVisibleBid:N0} kr",
                    _ => "Ett okänt fel inträffade när budet skulle läggas. Försök igen senare." // This is a catch-all for any other errors
                };
                var subMessage = result.Error switch
                {
                    PlaceBidError.None => $"Ditt maxbud är satt till {maxBidAmount:N0} kr",
                    PlaceBidError.AlreadyLeading => $"Ditt maxbud är ändrat till {maxBidAmount:N0} kr",
                    _ => "" 
                };

                return Json(new
                {
                    success = true,
                    isLeading,
                    previousLeadingBidderId,
                    maxBidAmount,
                    minimumBidAmount,
                    newLeadingVisibleBid,
                    bidCount,
                    headMessage,
                    subMessage
                });
            }
            else
            {
                // If the bid failed or didn't beat the leading bid, we return a failure message and relevant information

                var headMessage = result.Error switch
                {
                    PlaceBidError.BiddingNotAvailable => "Budgivning är inte tillgänglig för denna annons.",
                    PlaceBidError.BidTooLow => isLeading ? "Ditt maxbud får inte vara lägre än ditt ledande bud." : $"{maxBidAmount:N0} kr är för lågt. Lägg minst {minimumBidAmount:N0} kr.",
                    PlaceBidError.SameAsPrevious => "Ditt maxbud är samma belopp som ditt tidigare maxbud. Inga ändringar gjordes.",
                    PlaceBidError.CounteredViaMaxBid => "Du har blivit överbjuden via maxbud.",
                    PlaceBidError.MaxBidPlacedFirst => "Den ledande budgivaren lade sitt maxbud först.",
                    _ => "Ett okänt fel inträffade när budet skulle läggas. Försök igen senare." 
                };
                var subMessage = result.Error switch
                {
                    PlaceBidError.CounteredViaMaxBid => $"{newLeadingBidderUserName} leder med {newLeadingVisibleBid:N0} kr",
                    _ => "" 
                };

                return Json(new 
                { 
                    success = false,
                    isLeading,
                    previousLeadingBidderId,
                    maxBidAmount,
                    minimumBidAmount,
                    newLeadingVisibleBid,
                    bidCount,
                    headMessage,
                    subMessage
                });
            }
        }

        // Get ad preview of already created ads
        [HttpGet]
        [Route("admin/annons/{id}/preview")]
        public async Task<IActionResult> GetAdPreview(int id)
        {
            var ad = await _adService.GetAdByIdAsync(id);
            if (ad == null)
                return NotFound();

            // Mark the ad as seen by admin
            await _adService.MarkAsSeenByAdminAsync(id);

            var vm = new AdDetailsVM
            {
                Id = ad.Id,
                Title = ad.Title,
                Description = ad.Description,
                StartingPrice = ad.StartingPrice,
                MinimumEndPrice = ad.MinimumEndPrice,
                BuyNowPrice = ad.BuyNowPrice,   
                HeadImageURL = ad.Images.Where(img => img.IsMain).Select(img => img.Url).FirstOrDefault() ?? string.Empty,                
                VideoURL = ad.VideoURL,
                Images = ad.Images.ToList(),
                AddedAt = ad.AddedAt,
                AuctionEndDate = ad.AuctionEndDate,
                IsCompanySeller = ad.IsCompanySeller,
                AvailableForPickup = ad.AvailableForPickup,
                PickupLocation = ad.PickupLocation,
                Condition = ad.Condition,
                AdType = ad.AdType,
                UserName = ad.Advertiser.UserName,                
                //ShippingMethod = ad.ShippingMethod,
                //ShippingCost = ad.ShippingCost,                
                //PaymentMethods = ad.PaymentMethods.Select(pm => pm.Name).ToList(),
                Breadcrumbs = await _categoryService.GetCategoryBreadcrumbsAsync(ad.CategoryId),
                SubCategories = await _categoryService.GetSubCategoriesAsync(ad.CategoryId)
            };

            return PartialView("_AdPreviewPartial", vm);
        }

        // Preview when creating a new ad
        [HttpPost]
        [Route("skapa-annons/preview")]
        public async Task<IActionResult> PreviewAd([FromBody] AdFormVM formVm)
        {
            if (formVm == null)
            {
                return BadRequest("formVm is null – check JSON structure and types");
            }            

            try
            {
                var user = await _userManager.GetUserAsync(User);

                // Fetch category breadcrumbs and subcategories based on the selected category
                var crumbs = formVm.CategoryId > 0
                    ? await _categoryService.GetCategoryBreadcrumbsAsync(formVm.CategoryId)
                    : new List<CategoryForAdvertisement>();
                var subCategories = formVm.CategoryId > 0
                    ? await _categoryService.GetSubCategoriesAsync(formVm.CategoryId)
                    : new List<CategoryForAdvertisement>();

                // Fetch the image URLs from the form
                var urls = formVm.Images
                    .Where(i => !string.IsNullOrWhiteSpace(i.Url))
                    .Select(i => i.Url!)
                    .ToList();

                // Manual mapping to AdDetailsVM
                var previewVm = new AdDetailsVM
                {
                    Id = 0,
                    Title = formVm.Title ?? string.Empty,
                    Description = formVm.Description ?? string.Empty,
                    StartingPrice = formVm.StartingPrice,
                    MinimumEndPrice = formVm.MinimumEndPrice,
                    BuyNowPrice = formVm.BuyNowPrice,
                    HeadImageURL = urls.FirstOrDefault() ?? string.Empty,                    
                    Images = urls.Skip(1)
                      .Select(u => new AdvertisementImage { Url = u })
                      .ToList(),
                    VideoURL = formVm.VideoURL,
                    AddedAt = DateTime.Now,
                    AuctionEndDate = formVm.AuctionEndDate,
                    IsCompanySeller = formVm.IsCompanySeller,
                    AvailableForPickup = formVm.AvailableForPickup,
                    PickupLocation = formVm.PickupLocation ?? string.Empty,
                    Condition = formVm.Condition.HasValue ? formVm.Condition.Value : (Condition?)null,
                    AdType = formVm.AdType,
                    UserName = user?.UserName,
                    Breadcrumbs = crumbs,
                    SubCategories = subCategories,

                    //ShippingMethod = formVm.ShippingMethod ?? string.Empty,
                    //ShippingCost = null,
                    //PaymentMethods = formVm.PaymentMethods ?? new List<string>(),                
                };
                return PartialView("_AdPreviewPartial", previewVm);
            }
            catch (Exception ex)
            {
                // Logga felet
                _logger.LogError(ex, "Failed to render preview");
                // Returnera en enkel feltext eller JSON – men låt inte undantaget bubbla upp
                return StatusCode(500, "Kunde inte rendera förhandsgranskningen");
            }            
        }

        // Preview when editing an ad
        [HttpPost]
        [Route("redigera-annons/preview")]
        public async Task<IActionResult> PreviewEditAd([FromBody] AdFormVM formVm)
        {
            if (formVm == null)
            {
                return BadRequest("formVm is null – check JSON structure and types");
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);

                // Fetch category breadcrumbs and subcategories based on the selected category
                var crumbs = formVm.CategoryId > 0
                    ? await _categoryService.GetCategoryBreadcrumbsAsync(formVm.CategoryId)
                    : new List<CategoryForAdvertisement>();
                var subCategories = formVm.CategoryId > 0
                    ? await _categoryService.GetSubCategoriesAsync(formVm.CategoryId)
                    : new List<CategoryForAdvertisement>();


                // Manual mapping to AdDetailsVM
                var previewVm = new AdDetailsVM
                {
                    Id = 0,
                    Title = formVm.Title ?? string.Empty,
                    Description = formVm.Description ?? string.Empty,
                    StartingPrice = formVm.StartingPrice,
                    MinimumEndPrice = formVm.MinimumEndPrice,
                    BuyNowPrice = formVm.BuyNowPrice,
                    Images = formVm.Images?
                        .Where(i => !string.IsNullOrWhiteSpace(i.Url))
                        .Select(i => new AdvertisementImage { Url = i.Url! })
                        .ToList() ?? new List<AdvertisementImage>(),
                    VideoURL = formVm.VideoURL,
                    AddedAt = DateTime.Now,
                    AuctionEndDate = formVm.AuctionEndDate,
                    IsCompanySeller = formVm.IsCompanySeller,
                    AvailableForPickup = formVm.AvailableForPickup,
                    PickupLocation = formVm.PickupLocation ?? string.Empty,
                    Condition = formVm.Condition.HasValue ? formVm.Condition.Value : (Condition?)null,
                    AdType = formVm.AdType,
                    UserName = user?.UserName,
                    Breadcrumbs = crumbs,
                    SubCategories = subCategories,

                    //ShippingMethod = formVm.ShippingMethod ?? string.Empty,
                    //ShippingCost = null,
                    //PaymentMethods = formVm.PaymentMethods ?? new List<string>(),                
                };
                return PartialView("_AdPreviewPartial", previewVm);
            }
            catch (Exception ex)
            {
                // Logga felet
                _logger.LogError(ex, "Failed to render preview");
                // Returnera en enkel feltext eller JSON – men låt inte undantaget bubbla upp
                return StatusCode(500, "Kunde inte rendera förhandsgranskningen");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSubCategories(int? parentId)
        {
            var categories = await _categoryService.GetSubCategoriesAsync(parentId);
            
            var result = categories.Select(c => new {
                id = c.Id,
                name = c.Name,
                hasChildren = c.Subcategories != null && c.Subcategories.Any()
            });
            return Json(result);
        }

        [HttpPost("skapa-annons/upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file was sent.");

            // Ensure directory exists
            var folder = Path.Combine(_env.WebRootPath, "Images", "UserImages");
            Directory.CreateDirectory(folder);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var savePath = Path.Combine(folder, fileName);

            // Save to disk
            using var stream = new FileStream(savePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Build public URL
            var url = $"/Images/UserImages/{fileName}";

            // Return URL as JSON
            return Json(new { url });
        }

        [HttpPost("skapa-annons/delete-image")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(DeleteImageDto dto)
        {            
            var success = await _adService.DeleteImageAsync(dto.ImageUrl);
            if (!success)
                return NotFound(new { success = false, message = "Bilden hittades inte." });
            return Ok(new { success = true });
        }        
        public class DeleteImageDto
        {            
            public string ImageUrl { get; set; } = null!;
        }

        private async Task RefillDropdownsAsync(AdFormVM vm)
        {
            vm.ConditionOptions = SelectListHelper.GetConditionOptions();
            vm.AdTypeOptions = SelectListHelper.GetAdTypeOptions();
            ViewBag.AllCategoriesFlat = await _categoryService.GetAllCategoriesFlatAsync();
        }

        // Check if there are ads expiring today and notify the user
        // Called from javascript on site load and checked every day by way of a cache entry
        // The cache entry expires at midnight (local time) to ensure it checks again the next day
        public async Task<IActionResult> CheckAdsExpiringToday()
        {
            // Ensure the user is authenticated
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false });

            // Include today's date in cache key
            string today = DateTime.Now.Date.ToString("yyyy-MM-dd"); 
            string cacheKey = $"CheckAdsExpiringToday-{userId}-{today}";

            // Check if the cache already contains an entry for today
            if (_cache.TryGetValue(cacheKey, out _))
            {
                return Json(new { success = true, created = false });
            }

            // If not in cache, generate notifications for ads expiring today
            bool created = await _notificationService.GenerateNotificationsAdsExpiringTodayAsync(userId);

            // If notifications were created, send a real-time notification to the user
            if (created)
            {
                await _notificationHubContext.Clients.User(userId).SendAsync("ReceiveNotification");
            }

            // Cache entry expires at midnight (local time)
            // This ensures that the check is performed again the next day
            var midnight = DateTime.Today.AddDays(1); 
            var timeUntilMidnight = midnight - DateTime.Now;

            _cache.Set(cacheKey, true, timeUntilMidnight); 

            return Json(new { success = true, created });
        }

    }
}
