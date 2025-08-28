using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Auctionsite.Models.Database;
using Auctionsite.Models.VM;
using Microsoft.AspNetCore.Mvc;
using Auctionsite.Data;
using Auctionsite.Models;


namespace Auctionsite.Areas.Identity.Pages.Account.Manage
{ 
      public class MinaAnnonserModel : PageModel
      {
            private readonly UserManager<User> _userManager;
            private readonly ApplicationDbContext _context;

            public MinaAnnonserModel(UserManager<User> userManager, ApplicationDbContext context)
            {
                _userManager = userManager;
                _context = context;
            }

            [BindProperty(SupportsGet = true)]
            public int PageNumber { get; set; } = 1;
            private const int PageSize = 10;

            public UserOverviewViewModel Overview { get; set; }

            public async Task<IActionResult> OnGetAsync()
            {
                var user = await _userManager.Users
                    .Include(u => u.Advertisements)
                        .ThenInclude(a => a.Category)
                    .Include(u => u.Advertisements)
                        .ThenInclude(a => a.Images)
                    .Include(u => u.ReviewsReceived)
                       .ThenInclude(r => r.Reviewer)
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (user == null) return NotFound();

                var adQuery = user.Advertisements.AsQueryable();

                var totalCount = adQuery.Count();
                var pageAds = adQuery
                    .OrderByDescending(ad => ad.Id)
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                var adCardList = pageAds.Select(ad => new AdCardVM
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
                    IsFavourite = false,
                    ApprovedAt = ad.ApprovedAt,
                    IsRejected = ad.IsRejected
                }).ToList();


                Overview = new UserOverviewViewModel
                {
                    UserId = user.Id,
                    FullName = user.FirstName + " " + user.LastName,
                    Annonser = new PagedResult<AdCardVM>
                    {
                        Items = adCardList,
                        PageNumber = PageNumber,
                        PageSize = PageSize,
                        TotalItems = totalCount
                    },
                    Omdomen = user.ReviewsReceived.OrderByDescending(r => r.CreatedAt).ToList()
                };

                return Page();
            }
      }
}